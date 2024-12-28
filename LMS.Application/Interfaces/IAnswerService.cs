using LMS.Application.DTOs;

namespace LMS.Application.Interfaces
{
    public interface IAnswerService
    {
        /// <summary>
        /// Retrieves all answers.
        /// </summary>
        /// <returns>A collection of AnswerDTO objects.</returns>
        Task<IEnumerable<AnswerDTO>> GetAllAsync();

        /// <summary>
        /// Retrieves a specific answer by its ID.
        /// </summary>
        /// <param name="id">The ID of the answer.</param>
        /// <returns>An AnswerDTO object.</returns>
        Task<AnswerDTO> GetByIdAsync(int id);

        /// <summary>
        /// Adds a new answer.
        /// </summary>
        /// <param name="createAnswerDTO">The DTO containing answer data.</param>
        /// <returns>An asynchronous task.</returns>
        Task AddAsync(CreateAnswerDTO createAnswerDTO);

        /// <summary>
        /// Updates an existing answer.
        /// </summary>
        /// <param name="updateAnswerDTO">The DTO containing updated answer data.</param>
        /// <returns>An asynchronous task.</returns>
        Task UpdateAsync(UpdateAnswerDTO updateAnswerDTO);

        /// <summary>
        /// Deletes an answer by its ID.
        /// </summary>
        /// <param name="id">The ID of the answer to be deleted.</param>
        /// <returns>An asynchronous task.</returns>
        Task DeleteAsync(int id);
    }
}
