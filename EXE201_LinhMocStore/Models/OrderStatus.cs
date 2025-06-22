namespace EXE201_LinhMocStore.Models
{
    public static class OrderStatus
    {
        public const string PendingPayment = "PendingPayment";
        public const string AwaitingConfirmation = "AwaitingConfirmation";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Completed = "Completed";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";
        public const string Returned = "Returned";
        public const string Failed = "Failed";
    }
}
