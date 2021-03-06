using System;
using System.Net.Http;
using System.Threading.Tasks;
using SaferPay.Models;
using SaferPay.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace SaferPay.Real.Tests {
	public class IntegrationTests {
		private readonly ITestOutputHelper _output;

		public IntegrationTests(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public async Task FullTest1CHF()
		{
			var settings = TestSettings.LoadSettings();
			var card = TestSettings.LoadCreditCard();

			using( var client = new SaferPayClient( new HttpClient(), settings ) ) {
				_output.WriteLine( "Initialize" );
				var initializeResponse = await client.InitializeAsync( new InitializeRequest {
					TerminalId = settings.TerminalId,
					Payment = new InitializationPayment {
						Amount = new Amount {
							CurrencyCode = "CHF",
							Value = "100"
						},
					},
					PaymentMeans = new InitializationPaymentMeans {
						Card = card
					},
					ReturnUrls = new ReturnUrls {
						Success = new Uri( "http://localhost/success" ),
						Fail = new Uri( "http://localhost/fail" ),
						Abort = new Uri( "http://localhost/abort" )
					}
				} );

				Assert.NotNull( initializeResponse );
				Assert.NotNull( initializeResponse.Token );
				Assert.True( initializeResponse.Expiration > DateTimeOffset.UtcNow );






				_output.WriteLine( "Authorize" );
				var authorizeResponse = await client.AuthorizeAsync( new AuthorizeRequest {
					Token = initializeResponse.Token
				} );

				Assert.NotNull( authorizeResponse );
				Assert.NotNull( authorizeResponse.Transaction );
				Assert.NotNull( authorizeResponse.Transaction.Id );



				_output.WriteLine( "Capture" );
				var captureResponse = await client.CaptureAsync( new CaptureRequest {
					TransactionReference = new TransactionReference {
						TransactionId = authorizeResponse.Transaction.Id
					},
					Amount = new Amount {
						CurrencyCode = "CHF",
						Value = "100"
					},
				} );

				Assert.NotNull( captureResponse );
				Assert.NotNull( captureResponse.TransactionId );






				_output.WriteLine( "Refund" );
				var refundResponse = await client.RefundAsync( new RefundRequest {
					Refund = new Refund {
						Amount = new Amount {
							CurrencyCode = "CHF",
							Value = "100"
						}
					},
					TransactionReference = new TransactionReference {
						TransactionId = captureResponse.TransactionId
					}
				} );

				Assert.NotNull( refundResponse );
				Assert.NotNull( refundResponse.Transaction );
			}
		}
	}
}