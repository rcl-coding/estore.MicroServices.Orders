namespace estore.MicroServices.Orders.Models
{
    public class PaymentMessage
    {
        public string txn_id { get; set; } 

        public string payer_email { get; set; } 
        public string first_name { get; set; } 
        public string last_name { get; set; }

        public string address_street { get; set; }
        public string address_city { get; set; }
        public string address_state { get; set; }
        public string address_country { get; set; } 
        public string address_zip { get; set; } 

        public string custom { get; set; }

        public string item_number { get; set; }
        public string item_name { get; set; } 
        public string payment_gross { get; set; } 
        public string quantity { get; set; } 
    }
}
