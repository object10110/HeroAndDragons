using HeadWorks_task.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HeadWorks_task.Controllers
{
    public class HomeController : Controller
    {
        private HeroesAndDragonsContext _context;
        private static Random _random;
        private readonly string _tokenName = "accessToken";
        private readonly int MIN_NAME_LENGTH = 4;
        private readonly int MAX_NAME_LENGTH = 20;
        private readonly int MIN_DRAGON_NAME_LENGTH = 4;
        private readonly int MAX_DRAGON_NAME_LENGTH = 20;
        private readonly int MIN_DRAGON_NAME_SYLLABLE = 2;
        private readonly int MAX_DRAGON_NAME_SYLLABLE = 6;
        private readonly string NAME_PATTERN = @"^[a-zA-Z0-9]+$";
        private readonly int PAGE_SIZE = 10;
        public HomeController(HeroesAndDragonsContext context)
        {
            _context = context;
            _random = new Random();
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Heroes and Dragons";
            if (Request.Cookies[_tokenName] != null)
            {
                var id = GetHeroIdFromToken((Request.Cookies[_tokenName]));
                if (await _context.Heroes.AnyAsync(h => h.Id == id))//если пользователь залогинен
                {
                    return RedirectToAction("Dragons");              //уводим его со страницы регистрации
                }
            }
            return View("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Attack(int? dragonId)
        {
            if (dragonId != null && dragonId > 0)
            {
                var id = GetHeroIdFromToken((Request.Cookies[_tokenName]));
                var hero = _context.Heroes.ToList().FirstOrDefault(h => h.Id == id);
                if (hero != null)
                {
                    var dragon = await _context.Dragons.FirstOrDefaultAsync(d => d.Id == dragonId);
                    if (dragon != null)//если дракон найден в базе
                    {
                        var allDamageToDragon = await _context.Hits.Where(h => h.DragonId == dragon.Id).SumAsync(h => h.Damage);
                        if (dragon.Health > allDamageToDragon)//если дракон жив
                        {
                            var dragoInfo = new DragonInfoModel(dragon, allDamageToDragon, -1);
                            return View(dragoInfo);
                        }
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        //[Route("attackDragon")]
        [HttpPost("/home/AttackDragon")]
        public async Task<JsonResult> AttackDragon(int dragonId = -1)
        {
            if (dragonId > 0)
            {
                var id = GetHeroIdFromToken((Request.Cookies[_tokenName]));
                var hero = _context.Heroes.ToList().FirstOrDefault(h => h.Id == id);
                if (hero != null)
                {
                    var dragon = await _context.Dragons.FirstOrDefaultAsync(d => d.Id == dragonId);
                    if (dragon != null)//если дракон найден в базе
                    {
                        var allDamageToDragon = await _context.Hits.Where(h => h.DragonId == dragon.Id).SumAsync(h => h.Damage);
                        if (dragon.Health > allDamageToDragon)//если дракон жив
                        {
                            var hit = new Hit(hero, dragon, dragon.Health - allDamageToDragon);//создаем удар
                            await _context.Hits.AddAsync(hit);
                            await _context.SaveChangesAsync();
                            //var heroDamage = await _context.Hits.Where(h => h.DragonId == dragon.Id
                            //&& h.HeroId == hero.Id).SumAsync(h => h.Damage);// полный урон героя дракону
                            var dragoInfo = new DragonInfoModel(dragon, allDamageToDragon + hit.Damage, -1);
                            var view = dragoInfo.CurrentHealth == 0 ? await this.RenderViewAsync("PartialWin", dragoInfo.Name, true) : null;//если дракон убит передать представление для отображения победы
                            return Json(new { status = "ok", dragon = dragoInfo, damage = hit.Damage, view });
                            //hit.Damage - текущий урон дракону
                        }
                    }
                }
            }
            return Json(new { status = "error", message = "Ошибка!" });
        }

        [HttpGet]
        public async Task<IActionResult> Stat(int page = 1, SortState sortOrder = SortState.NameAsc)
        {
            var id = GetHeroIdFromToken((Request.Cookies[_tokenName]));
            var hero = _context.Heroes.ToList().FirstOrDefault(h => h.Id == id);
            if (hero != null)
            {
                IEnumerable<DragonInfoModel> dragons = GetDragonInfoList(await _context.Dragons.ToListAsync(), hero.Id).Where(d=> d.AmountHeroDamageForThisDragon>0);
                // сортировка
                switch (sortOrder)
                {
                    case SortState.NameDesc:
                        dragons = dragons.OrderByDescending(s => s.Name);
                        break;
                    case SortState.DamageDesc:
                        dragons = dragons.OrderByDescending(s => s.AmountHeroDamageForThisDragon);
                        break;
                    case SortState.DamageAsc:
                        dragons = dragons.OrderBy(s => s.AmountHeroDamageForThisDragon);
                        break;
                    default:
                        dragons = dragons.OrderBy(s => s.Name);
                        break;
                }
                // пагинация
                var count = dragons.Count();
                var items = dragons.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToList();
                // формируем модель представления
                DragonViewModel viewModel = new DragonViewModel
                {
                    PageViewModel = new PageViewModel(count, page, PAGE_SIZE),
                    SortViewModel = new SortModel(sortOrder),
                    Dragons = items
                };
                ViewData["menu-title"] = $"Статистика героя {hero.Name}";
                ViewData["hero"] = hero;
                return View(viewModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Dragons(string name, int minCurHealth = 0, int maxCurHealth = Dragon.MAX_HEALTH + 1,
                                        int minHealth = Dragon.MIN_HEALTH - 1, int maxHealth = Dragon.MAX_HEALTH + 1, //фильтры
                                        int page = 1,                                                         //пагинация
                                        SortState sortOrder = SortState.NameAsc)                              //сортировка
        {
            var id = GetHeroIdFromToken((Request.Cookies[_tokenName]));
            var hero = _context.Heroes.ToList().FirstOrDefault(h => h.Id == id);
            if (hero != null)
            {
                IEnumerable<DragonInfoModel> dragons = GetDragonInfoList(await _context.Dragons.ToListAsync(), hero.Id).Where(d=> d.CurrentHealth>0);
                //фильтрация
                if (!string.IsNullOrWhiteSpace(name))
                {
                    dragons = dragons.Where(p => p.Name.ToLower().StartsWith(name.ToLower()));//если имя начинается с указаной строки БЕЗ учета регистра
                }
                dragons = dragons.Where(d => d.CurrentHealth >= minCurHealth && d.CurrentHealth < maxCurHealth);//текущие жизни //>= для того, чтобы подпадали жизни равные 0
                dragons = dragons.Where(d => d.AllHealth > minHealth && d.CurrentHealth < maxHealth);//начальные жизни
                // сортировка
                switch (sortOrder)
                {
                    case SortState.NameDesc:
                        dragons = dragons.OrderByDescending(s => s.Name);
                        break;
                    default:
                        dragons = dragons.OrderBy(s => s.Name);
                        break;
                }
                // пагинация
                var count = dragons.Count();
                var items = dragons.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToList();
                // формируем модель представления
                DragonViewModel viewModel = new DragonViewModel
                {
                    PageViewModel = new PageViewModel(count, page, PAGE_SIZE),
                    SortViewModel = new SortModel(sortOrder),
                    FilterViewModel = new DragonFilterModel(name, minCurHealth, maxCurHealth, minHealth, maxHealth),
                    Dragons = items
                };
                ViewData["menu-title"] = "Драконы";
                ViewData["hero"] = hero;
                return View(viewModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> FindDragon()//добавляет случайного дракона
        {
            var id = GetHeroIdFromToken((Request.Cookies[_tokenName]));
            var hero = _context.Heroes.ToList().FirstOrDefault(h => h.Id == id);
            if (hero != null)
            {
                var newDragon = new Dragon(await GenerateDragonName());
                await _context.Dragons.AddAsync(newDragon);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dragons");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Heroes(string name = "", string startTime = "", string finishTime = "",//фильтр
                                                int page = 1,                                    //пагинация
                                                SortState sortOrder = SortState.NameAsc) //сортировка
        {
            var hero = _context.Heroes.ToList().FirstOrDefault(h => h.Id == GetHeroIdFromToken(Request.Cookies[_tokenName]));
            if (hero != null)//если удалось получить учетку пользователя
            {
                IQueryable<Hero> heroes = _context.Heroes;
                var selectedStartTime = _context.Heroes.Min(h => h.CreationTime);//устанавливаем минимальное значение фильтра времени
                var selectedFinishTime = _context.Heroes.Max(h => h.CreationTime);//устанавливаем максимальное значение фильтра времени
                //фильтрация
                if (!string.IsNullOrWhiteSpace(name))
                {
                    heroes = heroes.Where(p => p.Name.ToLower().StartsWith(name.ToLower()));//если имя начинается с указаной строки БЕЗ учета регистра
                }
                if (!string.IsNullOrEmpty(startTime))
                {
                    selectedStartTime = ParseStringTime(startTime);
                    heroes = heroes.Where(p => DateTime.Compare(p.CreationTime, selectedStartTime) > 0);//если создание героя позже указаной даты
                }
                if (!string.IsNullOrEmpty(finishTime))
                {
                    selectedFinishTime = ParseStringTime(finishTime);
                    heroes = heroes.Where(p => DateTime.Compare(p.CreationTime, selectedFinishTime) < 0);//если создание героя раньше указаной даты
                }
                // сортировка
                switch (sortOrder)
                {
                    case SortState.NameDesc:
                        heroes = heroes.OrderByDescending(s => s.Name);
                        break;
                    default:
                        heroes = heroes.OrderBy(s => s.Name);
                        break;
                }
                // пагинация
                var count = await heroes.CountAsync();
                var items = await heroes.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToListAsync();
                // формируем модель представления
                HeroesViewModel viewModel = new HeroesViewModel
                {
                    PageViewModel = new PageViewModel(count, page, PAGE_SIZE),
                    SortViewModel = new SortModel(sortOrder),
                    FilterViewModel = new HeroFilterModel(name, selectedStartTime, selectedFinishTime),
                    Heroes = items
                };
                ViewData["menu-title"] = "Герои";
                ViewData["hero"] = hero;
                return View(viewModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost("/home/login")]
        public async Task<JsonResult> Login(string username="")
        {
            username = username.Trim();
            if (!_context.Heroes.Any(h => h.Name.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                if (username.Length >= MIN_NAME_LENGTH && username.Length <= MAX_NAME_LENGTH)
                {
                    if (Regex.IsMatch(username, NAME_PATTERN))
                    {
                        var hero = new Hero(username);
                        await _context.Heroes.AddAsync(hero);
                        await _context.SaveChangesAsync();
                        var token = Token(hero.Id);
                        return Json(new { status = "ok", message =token });
                    }
                    else
                    {
                        return Json(new { status = "error", message = $"Имя должно состоять только из латинских символов и цифр!" });
                    }
                }
                else
                {
                    return Json(new { status = "error", message = $"Имя должно состоять от {MIN_NAME_LENGTH} до {MAX_NAME_LENGTH} символов!" });
                }
            }
            else
            {
                return Json(new { status = "error", message = $"Имя {username} ЗАНЯТО! Введите другое." });
            }
        }

        #region privateMethods
        private List<DragonInfoModel> GetDragonInfoList(List<Dragon> list, int heroId)
        {
            var listDI = new List<DragonInfoModel>();
            foreach (var item in list)
            {
                IEnumerable<Hit> hits = _context.Hits.Where(h => h.DragonId == item.Id).ToList();
                var allDragonDamage = hits.Sum(d => d.Damage);
                var heroDamage = hits.Where(h => h.HeroId == heroId).Sum(h => h.Damage);
                listDI.Add(new DragonInfoModel(item, allDragonDamage, heroDamage));
            }
            return listDI;
        }
        private async Task<string> GenerateDragonName()
        {
            NameGenerator nameGen = new NameGenerator();
            string dragonName;
            do
            {
                dragonName = nameGen.Compose(_random.Next(MIN_DRAGON_NAME_SYLLABLE, MAX_DRAGON_NAME_SYLLABLE + 1));
            } while (!await CheckDragonName(dragonName));
            return dragonName;
        }
        private async Task<bool> CheckDragonName(string name)
        {
            if (name.Length < MIN_DRAGON_NAME_LENGTH || name.Length > MAX_DRAGON_NAME_LENGTH)
            {
                return false;
            }
            if (await _context.Dragons.AnyAsync(d => d.Name.Equals(name)))
            {
                return false;
            }
            return true;
        }
        private string Token(int id)
        {
            var identity = GetIdentity(id);
            if (identity == null)
            {
                Response.StatusCode = 400;
                return null;
            }
            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
           
            return encodedJwt;
        }
        private ClaimsIdentity GetIdentity(int id)
        {
            Hero hero = _context.Heroes.FirstOrDefault(x => x.Id == id);
            if (hero != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, hero.Id.ToString()),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, hero.Id.ToString())
                };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,
                    "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }
            // если пользователя не найдено
            return null;
        }
        private int GetHeroIdFromToken(string token)
        {
            try
            {
                var t = Request.Cookies[_tokenName];
                var stream = t;
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(stream);
                var tokenS = handler.ReadToken(t) as JwtSecurityToken;
                return int.Parse(tokenS.Claims.ToList()[0].Value);
            }
            catch
            {
                return -1;
            }
        }
        private string GetDatetimeFromJStime(string time)
        {
            //time = time.Substring(2);//приходить год формата yyyy - изменяем на yy
            time = time.Replace("-", "/");// в html разделение часов через -, а в шаблоне DateTime через /
            time = time.Replace("T", " ");
            time = time + ":00";//добавление минут
            return time;
        }
        private DateTime ParseStringTime(string timeStr)
        {
            try
            {//если пришла строка с input[type=datetime-local]
                return DateTime.ParseExact(GetDatetimeFromJStime(timeStr), "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            catch
            {//если пришла строка с HeroFilterModel.SelectedTime
                return DateTime.ParseExact(GetDatetimeFromJStime(timeStr), "dd.MM.yyyy HH:mm:ss:ff", CultureInfo.InvariantCulture);
            }
        }
        #endregion
    }
}
