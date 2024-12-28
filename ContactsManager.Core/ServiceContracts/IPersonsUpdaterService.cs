using System;
using ServiceContracts.DTO;

namespace ContactsManager.Core.ServiceContracts
{
    /// <summary>
    /// Represents business logic (upload) for manipulating Perosn entity
    /// </summary>
    public interface IPersonsUpdaterService
    {
        /// <summary>
        /// Updates the specified person details based on the given person ID
        /// </summary>
        /// <param name="personUpdateRequest">Person details to update, including person id</param>
        /// <returns>Returns the person response object after updation</returns>
        Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);
    }
}
