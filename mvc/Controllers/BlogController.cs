/*
 * ahrensburg.city (MeinCMS) - A lightweight CMS with Wiki functionality and multi-tenancy.
 * Copyright (C) 2026 Thorsten
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.AspNetCore.Mvc;
using mvc.Services;
using System.Threading.Tasks;

namespace mvc.Controllers;

public class BlogController : Controller
{
    private readonly IPageService _pageService;

    public BlogController(IPageService pageService)
    {
        _pageService = pageService;
    }

    [HttpGet("Blog")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Blog";
        var articles = await _pageService.GetBlogArticlesAsync(50);
        return View(articles);
    }
}
