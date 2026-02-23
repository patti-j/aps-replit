using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WebAppImportService.Models
{
	internal class Instance
	{
		// TODO: username, password, and authtoken can be class members (authtoken will have to be added later than construction, after handshake/login, and be refreshed before calls)

		// TODO: abstract out the client creation stuff
		public static async Task<byte[]> CallHandshakeAPI(string publicKey, string url)
		{
			var httpClientHandler = new HttpClientHandler();
			// Return `true` to allow certificates that are untrusted/invalid
			httpClientHandler.ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
			using (HttpClient client = new HttpClient(httpClientHandler))
			{
				try
				{
					string route = $"{url}api/SystemServerActions/Handshake";
					UriBuilder uriBuilder = new UriBuilder(route);
					NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);
					query["a_publicKey"] = publicKey;
					uriBuilder.Query = query.ToString();
					route = uriBuilder.ToString();

					// Make the GET request to the API
					client.DefaultRequestHeaders.TransferEncodingChunked = false;
					HttpResponseMessage response = await client.GetAsync(route);

					return await GetResponseObject<byte[]>(response);
				}
				catch (Exception ex)
				{
					// Handle any exceptions that occur during the API call
					Console.WriteLine($"An error occurred: {ex.Message}");
					return null;
				}
			}
		}

		public static async Task<List<InstanceWithUser>> CallServerManagerToGetInstance(string ServerManagerURL, string UserName, string Password)
		{
			var httpClientHandler = new HttpClientHandler();
			// Return `true` to allow certificates that are untrusted/invalid
			httpClientHandler.ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
			using (HttpClient client = new HttpClient(httpClientHandler))
			{
				try
				{
					string jsonBody = $@"{{
                        ""Password"": ""{Password}"",
                        ""UserName"": ""{UserName}""
                    }}";
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ServerManagerURL);
					request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
					// Make the GET request to the API
					HttpResponseMessage response = await client.SendAsync(request);

					return await GetResponseObject<List<InstanceWithUser>>(response);
				}
				catch (Exception ex)
				{
					// Handle any exceptions that occur during the API call
					Console.WriteLine($"An error occurred: {ex.Message}");
					return null;
				}
			}
		}
		/// <summary>
		/// Byju: this method should work with any request you have to make, by providing the appropriate T of the expected response.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="response"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static async Task<T> GetResponseObject<T>(HttpResponseMessage response)
		{
			using (Stream stream = await response.Content.ReadAsStreamAsync())
			{

				if (response.IsSuccessStatusCode)
				{
					if (stream == null)
					{
						//throw new ApiException
						//{
						//    StatusCode = (int)response.StatusCode,
						//    Content = "No content for successful response"
						//};
						Console.WriteLine("No content for successful response");
					}

					if (!stream.CanRead)
					{
						//throw new ApiException
						//{
						//    StatusCode = (int)response.StatusCode,
						//    Content = "Invalid content for successful response"
						//};
						Console.WriteLine("Invalid content for successful response");
					}

					return DeserializeJsonFromStream<T>(stream);
				}

				//Error
				Task<string> task = StreamToStringAsync(stream);
				//throw new ApiException
				//{
				//    StatusCode = (int)response.StatusCode,
				//    Content = task.Result
				//};
				var error = $"Handshake unsuccessful. Status code {(int)response.StatusCode}: {task.Result}";
				Console.WriteLine(error);
				throw new Exception(error);
			}
		}

		private static T DeserializeJsonFromStream<T>(Stream a_stream)
		{
			using (var sr = new StreamReader(a_stream))
			{
				var json = sr.ReadToEnd();
				return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
			}
		}

		private static async Task<string> StreamToStringAsync(Stream a_stream)
		{
			if (a_stream != null)
			{
				using (StreamReader sr = new StreamReader(a_stream))
				{
					return await Task.Run(() => sr.ReadToEndAsync());
				}
			}

			return null;
		}

		public static async Task<UserLoginResponse> PostLogin(BasicLoginRequest request, string url)
		{
			var httpClientHandler = new HttpClientHandler();
			// Return `true` to allow certificates that are untrusted/invalid
			httpClientHandler.ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
			using (HttpClient client = new HttpClient(httpClientHandler))
			{
				try
				{
					string route = $"{url}api/Login/LoginAsUser";
					// Make the POST request to the API
					client.DefaultRequestHeaders.TransferEncodingChunked = false;
					HttpRequestMessage httpRequest = GeneratePostMessage(route, request);
					HttpResponseMessage response =
						await client.SendAsync(httpRequest, CancellationToken.None).ConfigureAwait(false);

					return await GetResponseObject<UserLoginResponse>(response);
				}
				catch (Exception ex)
				{
					// Handle any exceptions that occur during the API call
					Console.WriteLine($"An error occurred: {ex.Message}");
					return null;
				}
			}
		}

		public static HttpRequestMessage GeneratePostMessage(string a_endpoint, object a_object)
		{
			string content = JsonSerializer.Serialize(a_object);
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, a_endpoint);
			request.Content = new StringContent(content, Encoding.UTF8, "application/json");
			return request;
		}

		public static Task<GetScenarioResponse> GetScenarios(string username, string password, string sessionToken, string url)
		{
			GetScenarioRequest getScenarioRequest = new GetScenarioRequest();
			getScenarioRequest.ScenarioType = "";
			getScenarioRequest.TimeoutMinutes = 5;
			getScenarioRequest.UserName = username;
			getScenarioRequest.Password = password;
			getScenarioRequest.GetBlackBoxScenario = false;

			return PostGetScenarios(getScenarioRequest, sessionToken, url);
		}

		public static async Task<GetScenarioResponse> PostGetScenarios(GetScenarioRequest request, string sessionToken, string url)
		{
			var httpClientHandler = new HttpClientHandler();
			// Return `true` to allow certificates that are untrusted/invalid
			httpClientHandler.ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
			using (HttpClient client = new HttpClient(httpClientHandler))
			{
				try
				{
					string route = $"{url}api/ScenarioActions/GetScenarios";
					// Make the POST request to the API
					client.DefaultRequestHeaders.TransferEncodingChunked = false;
					AuthorizeClientForPTSessionAuth(sessionToken, client);
					HttpRequestMessage httpRequest = GeneratePostMessage(route, request);
					HttpResponseMessage response =
						await client.SendAsync(httpRequest, CancellationToken.None).ConfigureAwait(false);

					return await GetResponseObject<GetScenarioResponse>(response);
				}
				catch (Exception ex)
				{
					// Handle any exceptions that occur during the API call
					Console.WriteLine($"An error occurred: {ex.Message}");
					return null;
				}
			}
		}

		private static void AuthorizeClientForPTSessionAuth(string sessionToken, HttpClient client)
		{
			client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("SessionTokenScheme", sessionToken);
		}

		public static async Task<ImportResponse> ImportScenario(string ptUserName, string ptPassword, string ptAuthToken,
			ScenarioConfirmation scenario, string url)
		{
			ImportRequest importRequest = new ImportRequest
			{
				ScenarioId = scenario.ScenarioId,
				ScenarioName = scenario.ScenarioName,
				CreateScenarioIfNew = false,
				UserName = ptUserName,
				Password = ptPassword,
				TimeoutDuration = TimeSpan.FromSeconds(1200)
			};
			var x = await PostRequestImport(importRequest, ptAuthToken, url);
			return await PostRequestImport(importRequest, ptAuthToken, url);
		}

		public static async Task<ImportResponse> PostRequestImport(ImportRequest request, string sessionToken, string url)
		{
			var httpClientHandler = new HttpClientHandler();
			// Return `true` to allow certificates that are untrusted/invalid
			httpClientHandler.ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
			using (HttpClient client = new HttpClient(httpClientHandler))
			{
				try
				{
					string route = $"{url}api/ScenarioActions/api/APSWebervice/Import"; // Note: this route path is stupid

					// Make the POST request to the API
					client.DefaultRequestHeaders.TransferEncodingChunked = false;
					AuthorizeClientForPTSessionAuth(sessionToken, client);
					HttpRequestMessage httpRequest = GeneratePostMessage(route, request);
					HttpResponseMessage response =
						await client.SendAsync(httpRequest, CancellationToken.None).ConfigureAwait(false);
					//var x = GetResponseObject<ImportResponse>(response).Result;
					return await GetResponseObject<ImportResponse>(response);
				}
				catch (Exception ex)
				{
					// Handle any exceptions that occur during the API call
					Console.WriteLine($"An error occurred: {ex.Message}");
					return null;
				}
			}
		}
	}
}
