using ActiveCampaignAPIWrapper.Authentication;
using ActiveCampaignAPIWrapper.Helpers;
using ActiveCampaignAPIWrapper.Models;
using ActiveCampaignAPIWrapper.Services;

class Program
{
    static async Task Main(string[] args)
    {
        //ActiveCampaign API credentials
        const string apiKey = ""; //API Key for testing
        const string baseUrl = ""; //Base URL for testing

        // Initialize the ActiveCampaign Client

        var client = new ActiveCampaignClient(apiKey, baseUrl);

        // Initialize services
        var contactService = new ContactService(client);
        var customFieldService = new CustomFieldService(client);
        var tagService = new TagService(client);

        try
        {
            // 1. Create a new contact
            Console.WriteLine("Enter the contact email:");
            var email = Console.ReadLine();

            Console.WriteLine("Enter the contact first name:");
            var firstName = Console.ReadLine();

            Console.WriteLine("Enter the contact last name:");
            var lastName = Console.ReadLine();

            Console.WriteLine("Enter the contact phone number:");
            var phone = Console.ReadLine();

            var contactResponse = await contactService.CreateContactAsync(new Contact
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone
            });

            Console.WriteLine($"Result: {contactResponse.responseMessage}");

            if (contactResponse.ret == 1)
            {
                // Directly access the ID
                long contactId = contactResponse.Id.Value;

                // Create Contact object with ID
                var contact = new Contact { Id = contactId };

                // 2. Update contact details
                Console.WriteLine("Do you want to update the contact? (yes/no):");
                var updateResponse = Console.ReadLine().ToLower();

                if (updateResponse.ToLower() == "yes")
                {
                    // Get new details for update
                    Console.WriteLine("Enter the updated first name:");
                    contact.FirstName = Console.ReadLine();

                    Console.WriteLine("Enter the updated last name:");
                    contact.LastName = Console.ReadLine();

                    Console.WriteLine("Enter the updated email:");
                    contact.Email = Console.ReadLine();

                    Console.WriteLine("Enter the updated phone number:");
                    contact.Phone = Console.ReadLine();

                    var updateResponseMsg = await contactService.UpdateContactAsync(contact);
                    Console.WriteLine($"Result: {updateResponseMsg.responseMessage}");
                }
                else
                {
                    Console.WriteLine("Contact update cancelled.");
                }


                //  3.Custom field creation based on user input
                Console.WriteLine("Enter the custom field title:");
                var customFieldTitle = Console.ReadLine();

                Console.WriteLine("Enter the custom field description:");
                var customFieldDescription = Console.ReadLine();

                Console.WriteLine("Enter the custom field type (e.g., text, textarea, dropdown, multiselect, radio, checkbox, hidden, datetime, date):");
                var customFieldType = Console.ReadLine();

                string customFieldOptions = string.Empty;

                // If the field type is dropdown, multiselect, or radio, ask for options
                if (customFieldType == "dropdown" || customFieldType == "multiselect" || customFieldType == "radio")
                {
                    Console.WriteLine("Enter the options for the field (comma separated):");
                    customFieldOptions = Console.ReadLine();
                }

                // Create the custom field with the entered details
                var customFieldResponse = await customFieldService.CreateCustomFieldAsync(new CustomField
                {
                    Title = customFieldTitle,
                    Type = customFieldType,  // Field type (text, textarea, dropdown, etc.)
                    Description = customFieldDescription,
                    Options = customFieldOptions
                });

                // Output the result of the custom field creation
                Console.WriteLine($"Result:{customFieldResponse.responseMessage}");
                // Store the custom field ID directly into customFieldId
                long customFieldId = 0;
                if (customFieldResponse.ret == 1 && customFieldResponse.Id.HasValue)
                {
                    customFieldId = customFieldResponse.Id.Value;  // Store the ID in the customFieldId variable

                }
                else
                {
                    Console.WriteLine("Error: Could not retrieve the Custom Field ID.");
                    return; // Exit if the ID is not available
                }

                // 4. Update custom field value
                Console.WriteLine("Do you want to update the custom field value for the contact? (yes/no):");
                var updateCustomFieldResponse = Console.ReadLine().ToLower();
                if (updateCustomFieldResponse.ToLower() == "yes")
                {
                    Console.WriteLine($"Enter the value for the custom field '{customFieldTitle}':");
                    var customFieldValue = Console.ReadLine();

                    var customFieldUpdateResponse = await customFieldService.UpdateCustomFieldValueAsync(contact.Id, customFieldId, customFieldValue);
                    Console.WriteLine($"Result: {customFieldUpdateResponse.responseMessage}");
                }
                else
                {
                    Console.WriteLine("Update Custom Field cancelled.");
                }

                // 5. Add tags to contact
                Console.WriteLine("Enter comma-separated tag names (e.g. 'Customer, VIP, New Lead'): ");
                var tagNamesInput = Console.ReadLine();
                var tagNames = tagNamesInput.Split(',').Select(tag => tag.Trim()).ToList();

                var createdTagIds = new List<long>();

                foreach (var tagName in tagNames)
                {
                    Console.WriteLine($"Enter description for the tag '{tagName}':");
                    var tagDescription = Console.ReadLine();

                    var tagResponse = await tagService.CreateTagAsync(new Tag
                    {
                        Name = tagName,
                        Description = tagDescription
                    });
                    Console.WriteLine($"Result: {tagResponse.responseMessage}");

                    // Assuming the response has the Tag ID stored correctly
                    long tagId = tagResponse.Id.HasValue ? tagResponse.Id.Value : 0; // Using the ID directly from response

                    //var tagAddResponse = await tagService.AddTagToContactAsync(contact.Id, tagId);
                    //Console.WriteLine($"Result: {tagAddResponse.responseMessage}");

                    // Store the created tag Id
                    createdTagIds.Add(tagId);
                }

                // 6. Fetch activities for the contact
                Console.WriteLine("Do you want to fetch activities for the contact? (yes/no):");
                var fetchActivityResponse = Console.ReadLine().ToLower();
                if (fetchActivityResponse.ToLower() == "yes")
                {
                    var activities = await contactService.GetActivityForContactAsync(contact.Id);
                    Console.WriteLine($"Fetched {activities.Count} activities for contact ID {contact.Id}:");
                    foreach (var activity in activities)
                    {
                        Console.WriteLine($"Activity ID: {activity.Id}, Type: {activity.ActivityType}, Description: {activity.Description}, Created Date: {activity.CreatedDate}");
                    }
                }
                else
                {
                    Console.WriteLine("Fetch Activity Declined cancelled.");
                }


                // 7. Fetch activities for all contacts in a specific date range using helper method
                Console.WriteLine("Do you want to fetch activities for all contacts in a date range? (yes/no):");
                var fetchAllActivityResponse = Console.ReadLine().ToLower();
                if (fetchAllActivityResponse.ToLower() == "yes")
                {
                    Console.WriteLine("Enter the start date (yyyy-MM-dd):");
                    var startDate = DateTime.Parse(Console.ReadLine());

                    Console.WriteLine("Enter the end date (yyyy-MM-dd):");
                    var endDate = DateTime.Parse(Console.ReadLine());


                    // Now you can call the method
                    var activities = await contactService.GetActivityForAllContactsByDateRangeAsync(startDate, endDate);

                    Console.WriteLine($"Fetched {activities.Count} activities between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}:");

                    foreach (var activity in activities)
                    {
                        Console.WriteLine($"Activity ID: {activity.Id}, Type: {activity.ActivityType}, Description: {activity.Description}, Created Date: {activity.CreatedDate}");
                    }
                }

                else
                {
                    Console.WriteLine("fetch activities for all contacts in a date range cancelled.");
                }


                // 8. Delete the contact
                Console.WriteLine("Do you want to delete the contact? (yes/no):");
                var deleteResponse = Console.ReadLine().ToLower();
                if (deleteResponse.ToLower() == "yes")
                {
                    var deleteResponseMsg = await contactService.DeleteContactAsync(contact.Id);
                    Console.WriteLine($"Result: {deleteResponseMsg.responseMessage}");
                }
                else

                {
                    Console.WriteLine("Contact delete cancelled.");
                }
                // 9. Remove tags from contact
                Console.WriteLine("Do you want to remove tags from the contact? (yes/no):");
                var removeResponse = Console.ReadLine().ToLower();
                if (removeResponse.ToLower() == "yes")
                {
                    foreach (var tagId in createdTagIds)
                    {
                        var removeResponseMsg = await tagService.RemoveTagFromContactAsync(contact.Id, new List<long> { tagId });

                        Console.WriteLine($"Result: {removeResponseMsg.responseMessage}");
                    }
                }
                else
                {
                    Console.WriteLine("Remove Tag cancelled.");
                }
                // 10. Fetch contacts modified since a specific date
                var contactsModifiedSince = await contactService.GetContactsByModifiedDateAsync(new DateTime(2024, 1, 1));
                Console.WriteLine($"Contacts modified since 2024-01-01: {contactsModifiedSince.Count} contacts");

                // 11. Search contacts by email
                var searchResults = await contactService.SearchContactsAsync("software@outlook.com");
                Console.WriteLine($"Search result: {searchResults.Count} contacts found.");

                // 12. Get contact details by contact ID (including the contact's tags/custom fields)
                var contactResult = await contactService.GetContactByIdAsync(contact.Id);

                if (contactResult != null)
                {

                    Console.WriteLine($"First Name: {contactResult.FirstName}");
                    Console.WriteLine($"Last Name: {contactResult.LastName}");
                    Console.WriteLine($"Email: {contactResult.Email}");
                    Console.WriteLine($"Phone: {contactResult.Phone}");

                    // Display tags associated with the contact
                    Console.WriteLine("Tags associated with the contact:");
                    foreach (var tag in contactResult.Tags)
                    {
                        Console.WriteLine($"Tag Name: {tag.Name}, Tag ID: {tag.Id}");
                    }

                    // Display custom fields associated with the contact
                    Console.WriteLine("Custom fields associated with the contact:");
                    foreach (var customField in contactResult.CustomFields)
                    {
                        Console.WriteLine($"Custom Field Title: {customField.Title}, Value: {customField.Value}");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to fetch contact details.");
                }


            }
            else
            {
                //Console.WriteLine($"Error: {contactResponse.responseMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}