using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoanWeb.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DoanWeb.Controllers
{
    public class CareerGuideController : Controller
    {
        private readonly JobContext _context;

        public CareerGuideController(JobContext context)
        {
            _context = context;
        }

        // GET: /CareerGuide
        public async Task<IActionResult> Index()
        {
            var guides = await _context.CareerGuides
                .Where(g => g.IsActive == true)
                .OrderByDescending(g => g.PostedDate)
                .ToListAsync();

            return View(guides);
        }

        // GET: /CareerGuide/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var guide = await _context.CareerGuides.FindAsync(id);
            if (guide == null || guide.IsActive != true) return NotFound();

            var viewed = HttpContext.Session.GetString("ViewedGuides"); // e.g. "1,5,7"
            var list = string.IsNullOrEmpty(viewed) ? new List<int>() :
                       viewed.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

            if (!list.Contains(id))
            {
                // atomic update
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE CareerGuide SET Views = Views + 1 WHERE GuideID = {id}");

                list.Add(id);
                HttpContext.Session.SetString("ViewedGuides", string.Join(',', list));
            }

            // reload guide nếu cần để lấy view mới
            guide = await _context.CareerGuides.FindAsync(id);
            return View(guide);
        }

        // (Tùy chọn) Admin thêm bài viết
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CareerGuide guide)
        {
            if (ModelState.IsValid)
            {
                _context.CareerGuides.Add(guide);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(guide);
        }


        [HttpPost]
        public IActionResult UpdateOrder([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest();

            int order = 1;
            foreach (var id in ids)
            {
                var guide = _context.CareerGuides.Find(id);
                if (guide != null)
                {
                    guide.DisplayOrder = order++;
                }
            }

            _context.SaveChanges();
            return Ok();
        }

    }
}
