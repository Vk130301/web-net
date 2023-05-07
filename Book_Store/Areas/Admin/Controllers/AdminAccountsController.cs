using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Book_Store.Models;
using NToastNotify;
using Book_Store.Extension;
using Book_Store.Helpper;
using Book_Store.Areas.Admin.Models;
using Book_Store.Areas.Admin.Models.Authentication;
using PagedList.Core;

namespace Book_Store.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CheckAdmin]
    public class AdminAccountsController : Controller
    {
        private readonly QlBansachContext _context;
        private readonly IToastNotification _toastNotification;

        public AdminAccountsController(QlBansachContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        // GET: Admin/AdminAccounts
        public IActionResult Index(int page = 1,int RoleID = 0, int Active = 0)
        {
            var pageNumber = page;
            var pageSize = 10;

            List<Account> lsTrangThai = new List<Account>();
            if (RoleID != 0)
            {
                lsTrangThai = _context.Accounts
                .AsNoTracking()
                .Where(x => x.RoleId == RoleID)
                .Include(x => x.Role)
                .OrderBy(x => x.AccountId).ToList();
            }
            else
            {
                lsTrangThai = _context.Accounts
                .AsNoTracking()
                .Include(x => x.Role)
                .OrderBy(x => x.AccountId).ToList();
            }
            if (Active == 1 )
            {
                lsTrangThai = lsTrangThai.Where(x => x.Active == true ).ToList();
            }
            else if(Active == 2)
            {
                lsTrangThai = lsTrangThai.Where(x => x.Active == false).ToList();
            }


            PagedList<Account> models = new PagedList<Account>(lsTrangThai.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentRoleID = RoleID;
            ViewBag.CurrentActive = Active;
            ViewBag.CurrentPage = pageNumber;

            List<SelectListItem> lsStatus = new List<SelectListItem>();
            lsStatus.Add(new SelectListItem() { Text = "Active", Value = "1" });
            lsStatus.Add(new SelectListItem() { Text = "Disable", Value = "2" });
            foreach (var item in lsStatus)
            {
                if(item.Value == Active.ToString())
                {
                    item.Selected = true;
                    break;
                }
            }

            ViewData["lsStatus"] = lsStatus;
            ViewData["QuyenTruyCap"] = new SelectList(_context.Roles, "RoleId", "Description");
            return View(models);
        }

        public IActionResult Filter(int RoleID = 0, int Active = 0)
        {
            var url = $"/Admin/AdminAccounts/Index?RoleID={RoleID}&Active={Active}";
            if (RoleID == 0 & Active == 0)
            {
                url = $"/Admin/AdminAccounts/Index";
            }
            else
            {
                if (Active == 0) url = $"/Admin/AdminAccounts/Index?RoleID={RoleID}";
                if (RoleID == 0) url = $"/Admin/AdminAccounts/Index?Active={Active}";
            }
            return Json(new { status = "success", redirectUrl = url });
        }


        // GET: Admin/AdminAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Admin/AdminAccounts/Create
        public IActionResult Create()
        {
            ViewData["QuyenTruyCap"] = new SelectList(_context.Roles, "RoleId", "Description");
            return View();
        }

        // POST: Admin/AdminAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,Phone,Email,Password,Salt,Active,FullName,RoleId,LastLogin,CreateDate")] Account account)
        {
            if (ModelState.IsValid)
            {
                string salt = Utilities.GetRandomKey();
                account.Salt = salt;
                account.Password = (account.Password + salt.Trim()).ToMD5();
                account.CreateDate = DateTime.Now;

                _context.Add(account);
                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("Tạo mới tài khoản thành công");
                return RedirectToAction(nameof(Index));
            }
            ViewData["QuyenTruyCap"] = new SelectList(_context.Roles, "RoleId", "Description", account.RoleId);
            return View(account);
        }

        

        // GET: Admin/AdminAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            ViewData["QuyenTruyCap"] = new SelectList(_context.Roles, "RoleId", "RoleName", account.RoleId);
            return View(account);
        }

        // POST: Admin/AdminAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,Phone,Email,Password,Salt,Active,FullName,RoleId,LastLogin,CreateDate")] Account account)
        {
            if (id != account.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.AccountId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["QuyenTruyCap"] = new SelectList(_context.Roles, "RoleId", "RoleName", account.RoleId);
            return View(account);
        }

        // GET: Admin/AdminAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Admin/AdminAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Accounts == null)
            {
                return Problem("Entity set 'QlBansachContext.Accounts'  is null.");
            }
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
          return (_context.Accounts?.Any(e => e.AccountId == id)).GetValueOrDefault();
        }
    }
}
