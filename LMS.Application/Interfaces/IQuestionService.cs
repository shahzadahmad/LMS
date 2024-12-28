using LMS.Application.DTOs;

namespace LMS.Application.Interfaces
{
    public interface IQuestionService
    {
        /// <summary>
        /// Adds a new question.
        /// </summary>
        /// <param name="createQuestionDto">The data transfer object containing the information for the new question.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(CreateQuestionDTO createQuestionDto);

        /// <summary>
        /// Updates an existing question.
        /// </summary>
        /// <param name="updateQuestionDto">The data transfer object containing the updated information for the question.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(UpdateQuestionDTO updateQuestionDto);

        /// <summary>
        /// Deletes a question by its ID.
        /// </summary>
        /// <param name="id">The ID of the question to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(int id);

        /// <summary>
        /// Retrieves a question by its ID.
        /// </summary>
        /// <param name="id">The ID of the question to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, with a result of the question data transfer object.</returns>
        Task<QuestionDTO> GetQuestionByIdAsync(int id);

        /// <summary>
        /// Retrieves all questions.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a result of a collection of question data transfer objects.</returns>
        Task<IEnumerable<QuestionDTO>> GetAllQuestionsAsync();
    }
}
