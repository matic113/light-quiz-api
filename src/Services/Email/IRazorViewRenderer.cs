using System.Threading.Tasks;

namespace light_quiz_api.Services.Email
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
}