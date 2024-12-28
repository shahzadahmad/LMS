using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Application.Utilities;
using LMS.Common;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace LMS.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<UserService> _logger;
        private readonly IDistributedCache _cache;

        public UserService(
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IRoleRepository roleRepository,
            IOptions<JwtSettings> jwtSettings,
            ILogger<UserService> logger,
            IDistributedCache cache)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Authenticates a user by verifying their username and password.
        /// If authentication is successful, returns a UserDTO containing user details.
        /// </summary>
        /// <param name="username">The username of the user to authenticate.</param>
        /// <param name="password">The password of the user to authenticate.</param>
        /// <returns>A <see cref="UserDTO"/> with user details if authentication is successful; otherwise, <c>null</c>.</returns>
        public async Task<UserDTO> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                // Log the start of authentication process
                _logger.LogInformation($"Authenticating user: {username}");

                // Retrieve the user by username from the repository
                var user = await _userRepository.GetByUsernameAsync(username);

                // Check if user exists and password is correct
                if (user == null || !PasswordHasherUtility.VerifyPasswordHash(user.PasswordHash, password))
                {
                    // Log the authentication failure
                    _logger.LogWarning($"Authentication failed for user: {username}");
                    return null;
                }

                // Create and populate a UserDTO object with user details
                var userDto = new UserDTO
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Password = user.PasswordHash,
                    Email = user.Email,
                    FullName = user.FullName,
                    DateOfBirth = user.DateOfBirth,
                    Roles = user.UserRoles.Select(ur => new RoleDTO
                    {
                        RoleId = ur.RoleId,
                        RoleName = ur.Role.Name,
                        Description = ur.Role.Description
                    }).ToList(),
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                // Log successful authentication
                _logger.LogInformation($"User {username} authenticated successfully.");
                return userDto;
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, $"Error occurred while authenticating user: {username}");
                throw new ApplicationException("An error occurred while authenticating the user.", ex);
            }
        }

        /// <summary>
        /// Generates a JSON Web Token (JWT) for the specified user based on their claims and roles.
        /// </summary>
        /// <param name="user">The user for whom the JWT token is being generated.</param>
        /// <returns>A JWT token as a <see cref="string"/>.</returns>
        public string GenerateJwtToken(UserDTO user)
        {
            _logger.LogInformation($"Generating JWT token for user: {user.Username}");

            // Create a list of claims for the JWT token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            // Add roles as claims
            var roles = _userRoleRepository.GetRolesByUserIdAsync(user.UserId).Result;
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            // Create a security key and signing credentials
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: creds
            );

            // Convert the token to a string
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation($"JWT token generated for user: {user.Username}");
            return tokenString;
        }

        /// <summary>
        /// Retrieves all users from the database.
        /// Caches the result for subsequent requests to improve performance.
        /// </summary>
        /// <returns>A collection of <see cref="UserDTO"/> representing all users.</returns>
        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all users.");
                var cacheKey = "all_users";

                // Attempt to retrieve users from the cache
                var cachedUsers = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedUsers))
                {
                    _logger.LogInformation("Users retrieved from cache.");
                    return JsonSerializer.Deserialize<IEnumerable<UserDTO>>(cachedUsers);
                }

                // Retrieve users from the database
                var users = await _userRepository.GetAllAsync();
                if (users == null || !users.Any())
                {
                    _logger.LogInformation("No users found in the database.");
                    return Enumerable.Empty<UserDTO>();
                }

                // Map database entities to DTOs
                var userDtos = users.Select(user => new UserDTO
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    DateOfBirth = user.DateOfBirth,
                    Roles = user.UserRoles.Select(ur => new RoleDTO
                    {
                        RoleId = ur.RoleId,
                        RoleName = ur.Role.Name,
                        Description = ur.Role.Description
                    }).ToList(),
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }).ToList();

                // Cache the result for future requests
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)); // Cache expiration
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(userDtos), cacheOptions);

                _logger.LogInformation("Users retrieved and cached successfully.");
                return userDtos;
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "Error occurred while fetching all users.");
                throw new ApplicationException("An error occurred while fetching users.", ex);
            }
        }

        /// <summary>
        /// Retrieves a user by their ID from the database.
        /// Uses caching to speed up retrieval for subsequent requests.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>A <see cref="UserDTO"/> representing the user, or <c>null</c> if not found.</returns>
        public async Task<UserDTO> GetUserByIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Fetching user with ID {userId}.");
                var cacheKey = $"user_{userId}";

                // Attempt to retrieve user from the cache
                var cachedUser = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedUser))
                {
                    _logger.LogInformation($"User with ID {userId} retrieved from cache.");
                    return JsonSerializer.Deserialize<UserDTO>(cachedUser);
                }

                // Retrieve user from the database
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found.");
                    return null;
                }

                // Map database entity to DTO
                var userDto = new UserDTO
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    DateOfBirth = user.DateOfBirth,
                    Roles = user.UserRoles.Select(ur => new RoleDTO
                    {
                        RoleId = ur.RoleId,
                        RoleName = ur.Role.Name,
                        Description = ur.Role.Description
                    }).ToList(),
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                // Cache the result for future requests
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(userDto), cacheOptions);

                _logger.LogInformation($"User with ID {userId} retrieved and cached successfully.");
                return userDto;
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, $"Error occurred while fetching user with ID {userId}.");
                throw new ApplicationException($"An error occurred while fetching the user with ID {userId}.", ex);
            }
        }

        /// <summary>
        /// Retrieves a user by their email address from the database.
        /// Uses caching to speed up retrieval for subsequent requests.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>A <see cref="UserDTO"/> representing the user, or <c>null</c> if not found.</returns>
        public async Task<UserDTO> GetUserByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Fetching user with email: {email}.");
                var cacheKey = $"user_email_{email}";

                // Attempt to retrieve user from the cache
                var cachedUser = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedUser))
                {
                    _logger.LogInformation($"User with email {email} retrieved from cache.");
                    return JsonSerializer.Deserialize<UserDTO>(cachedUser);
                }

                // Retrieve user from the database
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning($"User with email {email} not found.");
                    return null;
                }

                // Map database entity to DTO
                var userDto = new UserDTO
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    DateOfBirth = user.DateOfBirth,
                    Roles = user.UserRoles.Select(ur => new RoleDTO
                    {
                        RoleId = ur.RoleId,
                        RoleName = ur.Role.Name,
                        Description = ur.Role.Description
                    }).ToList(),
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                // Cache the result for future requests
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(userDto), cacheOptions);

                _logger.LogInformation($"User with email {email} retrieved and cached successfully.");
                return userDto;
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, $"Error occurred while fetching user with email {email}.");
                throw new ApplicationException($"An error occurred while fetching the user with email {email}.", ex);
            }
        }

        /// <summary>
        /// Creates a new user in the database.
        /// Encrypts the password and assigns default roles.
        /// </summary>
        /// <param name="createUserDto">The <see cref="UserDTO"/> containing user details.</param>
        /// <returns>The created <see cref="UserDTO"/> including the newly assigned ID.</returns>
        public async Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDto)
        {
            try
            {
                _logger.LogInformation($"Creating new user: {createUserDto.Username}");

                // Hash the user's password
                createUserDto.Password = PasswordHasherUtility.HashPassword(createUserDto.Password);

                // Map DTO to entity
                var user = new User
                {
                    Username = createUserDto.Username,
                    PasswordHash = createUserDto.Password,
                    Email = createUserDto.Email,
                    FullName = createUserDto.FullName,
                    DateOfBirth = createUserDto.DateOfBirth,
                    CreatedAt = DateTime.UtcNow                    
                };

                // Save user to the database
                var createdUser = await _userRepository.AddAsync(user);

                // Assign roles based on the Roles list in the DTO
                if (createUserDto.Roles != null && createUserDto.Roles.Any())
                {
                    foreach (var roleDto in createUserDto.Roles)
                    {
                        var role = await _roleRepository.GetByIdAsync(roleDto.RoleId);
                        if (role != null)
                        {
                            await _userRoleRepository.AddAsync(new UserRole
                            {
                                UserId = createdUser.UserId,
                                RoleId = role.RoleId
                            });
                        }
                        else
                        {
                            _logger.LogWarning($"Role with ID {roleDto.RoleId} not found.");
                        }
                    }
                }
                else
                {
                    // If no roles are provided, assign the default "Student" role
                    var defaultRole = await _roleRepository.GetRoleByNameAsync("Student");
                    if (defaultRole != null)
                    {
                        await _userRoleRepository.AddAsync(new UserRole
                        {
                            UserId = createdUser.UserId,
                            RoleId = defaultRole.RoleId
                        });
                    }
                }

                // Refresh the user entity to include roles
                createdUser = await _userRepository.GetByIdAsync(createdUser.UserId);

                // Map entity to DTO
                var createdUserDto = new UserDTO
                {
                    UserId = createdUser.UserId,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    FullName = createdUser.FullName,
                    DateOfBirth = createdUser.DateOfBirth,
                    Roles = createdUser.UserRoles.Select(ur => new RoleDTO
                    {
                        RoleId = ur.RoleId,
                        RoleName = ur.Role.Name,
                        Description = ur.Role.Description
                    }).ToList(),
                    CreatedAt = createdUser.CreatedAt,
                    UpdatedAt = createdUser.UpdatedAt
                };

                _logger.LogInformation($"User {createUserDto.Username} created successfully.");
                return createdUserDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while creating user: {createUserDto.Username}");
                throw new ApplicationException($"An error occurred while creating the user: {createUserDto.Username}.", ex);
            }
        }

        /// <summary>
        /// Updates an existing user in the database.
        /// Only updates fields provided in the <see cref="UserDTO"/>.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="updateUserDto">The <see cref="UserDTO"/> containing updated user details.</param>
        /// <returns>The updated <see cref="UserDTO"/>.</returns>
        public async Task<UserDTO> UpdateUserAsync(int userId, UpdateUserDTO updateUserDto)
        {
            try
            {
                _logger.LogInformation($"Updating user with ID {userId}");

                // Retrieve the existing user from the database
                var existingUser = await _userRepository.GetByIdAsync(userId);
                if (existingUser == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found.");
                    return null;
                }

                // Update user details
                existingUser.Username = updateUserDto.Username ?? existingUser.Username;
                existingUser.Email = updateUserDto.Email ?? existingUser.Email;
                existingUser.FullName = updateUserDto.FullName ?? existingUser.FullName;
                existingUser.DateOfBirth = updateUserDto.DateOfBirth ?? existingUser.DateOfBirth;
                existingUser.UpdatedAt = DateTime.UtcNow;

                // Update password if provided
                if (!string.IsNullOrEmpty(updateUserDto.Password))
                {
                    existingUser.PasswordHash = PasswordHasherUtility.HashPassword(updateUserDto.Password);
                }

                // Save the updated user to the database
                await _userRepository.UpdateAsync(existingUser);

                // Map entity to DTO
                var updatedUserDto = new UserDTO
                {
                    UserId = existingUser.UserId,
                    Username = existingUser.Username,
                    Email = existingUser.Email,
                    FullName = existingUser.FullName,
                    DateOfBirth = existingUser.DateOfBirth,
                    Roles = existingUser.UserRoles.Select(ur => new RoleDTO
                    {
                        RoleId = ur.RoleId,
                        RoleName = ur.Role.Name,
                        Description = ur.Role.Description
                    }).ToList(),
                    CreatedAt = existingUser.CreatedAt,
                    UpdatedAt = existingUser.UpdatedAt
                };

                _logger.LogInformation($"User with ID {userId} updated successfully.");
                return updatedUserDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating user with ID {userId}");
                throw new ApplicationException($"An error occurred while updating the user with ID {userId}.", ex);
            }
        }

        /// <summary>
        /// Deletes a user from the database by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <returns><c>true</c> if the user was successfully deleted; otherwise, <c>false</c>.</returns>
        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Deleting user with ID {userId}");

                // Retrieve the existing user from the database
                var existingUser = await _userRepository.GetByIdAsync(userId);
                if (existingUser == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found.");
                    return false;
                }

                // Delete the user from the database
                await _userRepository.DeleteAsync(existingUser.UserId);

                // Invalidate cache for this user
                var cacheKey = $"user_{userId}";
                await _cache.RemoveAsync(cacheKey);

                _logger.LogInformation($"User with ID {userId} deleted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting user with ID {userId}");
                throw new ApplicationException($"An error occurred while deleting the user with ID {userId}.", ex);
            }
        }

        /// <summary>
        /// This method assign multipe role to a user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleIds"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task AssignRolesAsync(int userId, List<int> roleIds)
        {
            try
            {
                // Log the start of the role assignment process for the specified user
                _logger.LogInformation($"Assigning roles to user with ID {userId}.");

                // Retrieve the user by their ID from the database
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    // If the user is not found, log a warning and throw an exception
                    _logger.LogWarning($"User with ID {userId} not found.");
                    throw new KeyNotFoundException("User not found.");
                }

                // Remove all existing roles associated with the user
                await _userRoleRepository.DeleteRolesByUserIdAsync(userId);

                // Iterate through each role ID provided in the roleIds list
                foreach (var roleId in roleIds)
                {
                    // Retrieve the role by its ID from the database
                    var role = await _roleRepository.GetByIdAsync(roleId);
                    if (role == null)
                    {
                        // If the role is not found, log a warning and throw an exception
                        _logger.LogWarning($"Role with ID {roleId} not found.");
                        throw new KeyNotFoundException($"Role with ID {roleId} not found.");
                    }

                    // Create a new UserRole entity to link the user with the role
                    var userRole = new UserRole
                    {
                        UserId = userId,
                        RoleId = roleId
                    };

                    // Add the new UserRole entity to the database
                    await _userRoleRepository.AddAsync(userRole);
                }

                // Log the successful completion of the role assignment process
                _logger.LogInformation($"Roles assigned to user with ID {userId} successfully.");

                // Invalidate the cache for this user to ensure fresh data is retrieved next time
                await _cache.RemoveAsync($"user_{userId}");
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during the role assignment process
                _logger.LogError(ex, $"Error occurred while assigning roles to user with ID {userId}.");
                // Re-throw the exception to allow it to be handled further up the call stack
                throw;
            }
        }

    }
}