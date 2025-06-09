namespace EXE201_LinhMocStore.Services
{
    public class BankSettings
    {
        public string Bank_id { get; set; }
        public string AccountNo { get; set; }
        public string BankName { get; set; }

        public BankSettings(IConfiguration configuration)
        {
            Bank_id = configuration["Bank:BankId"];
            AccountNo = configuration["Bank:AccountNo"];
            BankName = configuration["Bank:AccountName"];
        }
    }
}
