using System.Threading.Tasks;
using Microsoft.Owin;

namespace Tests.Infrastructure
{
  public static class OwinExtensions
  {
    public static Task Response(this IOwinContext env, int statusCode, int consulIndex = -1, string body = "")
    {
      env.Response.StatusCode = statusCode;
      if (consulIndex >= 0)
        env.Response.Headers["X-Consul-Index"] = consulIndex.ToString();

      return body == null ? env.Response.Body.FlushAsync() : env.Response.WriteAsync(body);
    }
  }
}