using Persistence.Entities;

namespace Persistence.Context
{
    public class DataSeeder
    {
        private readonly DataContext _dataContext;

        public DataSeeder(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public void Seed()
        {
            var client = new Client()
            {
                Id = "5fa5a3a7-8a88-4b80-a7a5-d24a77132640",
                FirstName = "Smith",
                LastName = "John",
                Email = "john@gmail.com",
                PhoneNumber = "+18202820232",
            };

            _dataContext.Add(client);
            _dataContext.SaveChanges();
        }
    }
}