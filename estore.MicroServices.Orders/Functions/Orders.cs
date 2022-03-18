#nullable disable
using System.Net;
using System.Text.Json;
using estore.MicroServices.Orders.DataContext;
using estore.MicroServices.Orders.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;

namespace estore.MicroServices.Orders.Functions
{
    public class Orders
    {
        private readonly OrderDbContext _context;

        public Orders(OrderDbContext context)
        {
            _context = context;
        }

        [Function("Order_GetAll_V1")]
        public async Task<HttpResponseData> RunGetAllV1([HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/order/all")] HttpRequestData req)
        {
            try
            {
                List<Order> _orders = await _context.Orders.ToListAsync();

                if (_orders?.Count > 0)
                {
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    response.Headers.Add("Content-Type", "application/json ; charset=utf-8");
                    response.WriteString(JsonSerializer.Serialize(_orders));
                    return response;

                }
                else
                {
                    var response = req.CreateResponse(HttpStatusCode.NotFound);
                    return response;
                }

            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain ; charset=utf-8");
                response.WriteString(ex.Message);
                return response;
            }
        }

        [Function("Order_GetById_V1")]
        public async Task<HttpResponseData> RunGetByIdV1([HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/order/id/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                Order _order = await _context.Orders.FindAsync(id);

                if (_order != null)
                {
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    response.Headers.Add("Content-Type", "application/json ; charset=utf-8");
                    response.WriteString(JsonSerializer.Serialize(_order));
                    return response;

                }
                else
                {
                    var response = req.CreateResponse(HttpStatusCode.NotFound);
                    return response;
                }

            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain ; charset=utf-8");
                response.WriteString(ex.Message);
                return response;
            }
        }

        [Function("Order_GetByCustomerId_V1")]
        public async Task<HttpResponseData> RunGetByCustomerIdV1([HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/order/customer/{id}")] HttpRequestData req, string id)
        {
            try
            {
                List<Order> _orders = await _context.Orders.Where(w => w.CustomerId == id).ToListAsync();

                if (_orders?.Count > 0)
                {
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    response.Headers.Add("Content-Type", "application/json ; charset=utf-8");
                    response.WriteString(JsonSerializer.Serialize(_orders));
                    return response;

                }
                else
                {
                    var response = req.CreateResponse(HttpStatusCode.NotFound);
                    return response;
                }

            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain ; charset=utf-8");
                response.WriteString(ex.Message);
                return response;
            }
        }

        [Function("Order_CreateOrUpdate_V1")]
        public async Task<HttpResponseData> RunCreateOrUpdateV1([HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/order")] HttpRequestData req)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                if (!string.IsNullOrEmpty(requestBody))
                {
                    Order _order = JsonSerializer.Deserialize<Order>(requestBody);

                    if (_order.Id < 1)
                    {
                        _context.Orders.Add(_order);
                        await _context.SaveChangesAsync();

                        var response = req.CreateResponse(HttpStatusCode.OK);
                        response.Headers.Add("Content-Type", "application/json ; charset=utf-8");
                        response.WriteString(JsonSerializer.Serialize(_order));
                        return response;
                    }
                    else
                    {
                        Order existingOrder = await _context
                            .Orders.Where(w => w.Id == _order.Id)
                            .AsNoTracking().FirstOrDefaultAsync();

                        if (existingOrder != null)
                        {
                            _context.Attach(_order).State = EntityState.Modified;
                            await _context.SaveChangesAsync();

                            var response = req.CreateResponse(HttpStatusCode.OK);
                            response.Headers.Add("Content-Type", "application/json ; charset=utf-8");
                            response.WriteString(JsonSerializer.Serialize(_order));
                            return response;
                        }
                        else
                        {
                            var response = req.CreateResponse(HttpStatusCode.NotFound);
                            return response;
                        }
                    }
                }
                else
                {
                    var response = req.CreateResponse(HttpStatusCode.BadRequest);
                    response.Headers.Add("Content-Type", "text/plain ; charset=utf-8");
                    response.WriteString(JsonSerializer.Serialize("The product entity was not received in the body of the request"));
                    return response;
                }
            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain ; charset=utf-8");
                response.WriteString(ex.Message);
                return response;
            }
        }

        [Function("Order_Delete_V1")]
        public async Task<HttpResponseData> RunDeleteV1([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "v1/order/id/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                Order _order = await _context.Orders.FindAsync(id);

                if (_order != null)
                {
                    _context.Orders.Remove(_order);
                    await _context.SaveChangesAsync();

                    var response = req.CreateResponse(HttpStatusCode.OK);
                    return response;

                }
                else
                {
                    var response = req.CreateResponse(HttpStatusCode.NotFound);
                    return response;
                }

            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain ; charset=utf-8");
                response.WriteString(ex.Message);
                return response;
            }
        }

        [Function("Order_PaymentReceived_V1")]
        public async Task<HttpResponseData> RunPaymentReceivedV1([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/order/payment")] HttpRequestData req)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                if (!string.IsNullOrEmpty(requestBody))
                {
                    PaymentMessage _payment = JsonSerializer.Deserialize<PaymentMessage>(requestBody);

                    Order _order = new Order
                    {
                        CustomerId = _payment.custom,
                        CustomerEmail = _payment.payer_email,
                        CustomerName = $"{_payment.first_name} {_payment.last_name}",
                        Date = DateTime.Now,
                        ProductId = Convert.ToInt32(_payment.item_number),
                        ProductName = _payment.item_name,
                        Amount = Convert.ToDecimal(_payment.payment_gross),
                        Quantity = Convert.ToInt32(_payment.quantity),
                        TransactionId = _payment.txn_id,
                        ShippingAddress = $"{_payment.address_street} {_payment.address_city} {_payment.address_state} {_payment.address_country} {_payment.address_zip}"
                    };

                    decimal unitCost = Convert.ToDecimal(_order.Amount / _order.Quantity);
                    _order.UnitCost = Math.Round(unitCost, 2);

                    _context.Orders.Add(_order);
                    await _context.SaveChangesAsync();

                    var response = req.CreateResponse(HttpStatusCode.OK);
                    response.Headers.Add("Content-Type", "application/json ; charset=utf-8");
                    response.WriteString(JsonSerializer.Serialize(_order));
                    return response;
                }
                else
                {
                    var response = req.CreateResponse(HttpStatusCode.BadRequest);
                    response.Headers.Add("Content-Type", "text/plain ; charset=utf-8");
                    response.WriteString(JsonSerializer.Serialize("The product entity was not received in the body of the request"));
                    return response;
                }
            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain ; charset=utf-8");
                response.WriteString(ex.Message);
                return response;
            }
        }
    }
}
