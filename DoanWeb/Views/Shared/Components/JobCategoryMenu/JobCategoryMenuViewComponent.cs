using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoanWeb.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DoanWeb.ViewComponents
{
    public class JobCategoryMenuViewComponent : ViewComponent
    {
        private readonly JobContext _context;
        public JobCategoryMenuViewComponent(JobContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.JobCategories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            // Chia thành 2 cột để hiển thị dropdown
            int half = (categories.Count + 1) / 2;
            var col1 = categories.Take(half).ToList();
            var col2 = categories.Skip(half).ToList();

            var model = new Tuple<List<JobCategory>, List<JobCategory>>(col1, col2);

            return View(model);
        }
    }
}
