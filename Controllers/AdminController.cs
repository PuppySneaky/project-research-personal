using Film_website.Models;
using Film_website.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;

namespace Film_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserService _userService;
        private readonly UserActivityService? _activityService;
        private readonly ILogger<AdminController> _logger;
        private readonly MovieService _movieService;

        public AdminController(UserService userService, ILogger<AdminController> logger, MovieService movieService, UserActivityService? activityService = null)
        {
            _userService = userService;
            _logger = logger;
            _movieService = movieService;
            _activityService = activityService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Log admin access if activity service is available
                if (_activityService != null && User.Identity?.Name != null)
                {
                    var adminUser = await _userService.GetUserByEmailOrUserNameAsync(User.Identity.Name);
                    if (adminUser != null)
                    {
                        await _activityService.LogAdminAccessAsync(adminUser.Id, "Accessed Admin Index", HttpContext);
                    }
                }

                // Get all users with their roles for user management section
                var usersWithRoles = await _userService.GetAllUsersWithRolesDictionaryAsync();

                ViewBag.Message = "Chào mừng đến trang quản trị";
                ViewBag.TotalUsers = usersWithRoles.Count;

                return View(usersWithRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải danh sách người dùng.";
                ViewBag.Message = "Chào mừng đến trang quản trị";
                return View(new Dictionary<Film_website.Models.User, IList<string>>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserActivities(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID is required");
                }

                // If activity service is not available, return empty result
                if (_activityService == null)
                {
                    return Json(new
                    {
                        user = new { id = userId, name = "Unknown", username = "unknown", email = "unknown" },
                        activities = new object[0],
                        totalCount = 0,
                        message = "Activity tracking is not yet configured"
                    });
                }

                // Log admin action
                if (User.Identity?.Name != null)
                {
                    var adminUser = await _userService.GetUserByEmailOrUserNameAsync(User.Identity.Name);
                    if (adminUser != null)
                    {
                        await _activityService.LogAdminAccessAsync(adminUser.Id, $"Viewed activity log for user {userId}", HttpContext);
                    }
                }

                var activities = await _activityService.GetRecentActivitiesAsync(userId, 50);
                var totalCount = await _activityService.GetTotalUserActivitiesCountAsync(userId);

                // Get user info for display - Find user by ID from all users
                var allUsers = await _userService.GetAllUsersAsync();
                var user = allUsers.FirstOrDefault(u => u.Id == userId);

                var response = new
                {
                    user = user != null ? new
                    {
                        id = user.Id,
                        name = $"{user.FirstName} {user.LastName}",
                        username = user.DisplayUserName,
                        email = user.Email
                    } : null,
                    activities = activities.Select(a => new {
                        id = a.Id,
                        activityType = a.ActivityType,
                        description = a.Description,
                        createdAt = a.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                        ipAddress = a.IpAddress,
                        location = a.Location ?? "Unknown"
                    }),
                    totalCount = totalCount
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving activities for user {userId}");
                return StatusCode(500, "Error retrieving user activities");
            }
        }

        // Movie Management Methods
        public async Task<IActionResult> ManageMovies()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            return View(movies);
        }

        [HttpGet]
        public IActionResult AddMovie()
        {
            return View(new Movie());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMovie(Movie movie, IFormFile movieFile, IFormFile thumbnailFile, IFormFile subtitleFile)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = "Invalid input: " + string.Join("; ", errors);
                return View(movie);
            }

            try
            {
                await _movieService.AddMovieAsync(movie, movieFile, thumbnailFile, subtitleFile);
                TempData["Success"] = "Movie added successfully!";
                return RedirectToAction("ManageMovies");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error adding movie: {ex.Message}";
                return View(movie);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditMovie(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
                return NotFound();
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMovie(Movie movie, IFormFile movieFile, IFormFile thumbnailFile, IFormFile subtitleFile)
        {
            if (ModelState.IsValid)
            {
                // Fetch the tracked entity from the database
                var existingMovie = await _movieService.GetMovieByIdAsync(movie.Id);
                if (existingMovie == null)
                    return NotFound();

                // Update only the properties that are editable
                existingMovie.Title = movie.Title;
                existingMovie.Description = movie.Description;
                existingMovie.Genre = movie.Genre;
                existingMovie.ReleaseYear = movie.ReleaseYear;

                await _movieService.UpdateMovieAsync(existingMovie, movieFile, thumbnailFile, subtitleFile);
                TempData["Success"] = "Movie updated successfully!";
                return RedirectToAction("ManageMovies");
            }
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            await _movieService.DeleteMovieAsync(id);
            TempData["Success"] = "Movie deleted successfully!";
            return RedirectToAction("ManageMovies");
        }

        public async Task<IActionResult> ViewMovie(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
                return NotFound();
            return View(movie);
        }

        // Dashboard Methods
        public IActionResult Dashboard()
        {
            return View();
        }

        // User Management Methods
        public async Task<IActionResult> UserManagement()
        {
            try
            {
                // Log admin access
                if (_activityService != null && User.Identity?.Name != null)
                {
                    var adminUser = await _userService.GetUserByEmailOrUserNameAsync(User.Identity.Name);
                    if (adminUser != null)
                    {
                        await _activityService.LogAdminAccessAsync(adminUser.Id, "Accessed User Management", HttpContext);
                    }
                }

                var usersWithRoles = await _userService.GetAllUsersWithRolesDictionaryAsync();
                return View(usersWithRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user management");
                TempData["Error"] = "Error loading user management.";
                return RedirectToAction("Index");
            }
        }

        // System Settings
        public async Task<IActionResult> SystemSettings()
        {
            try
            {
                // Log admin access
                if (_activityService != null && User.Identity?.Name != null)
                {
                    var adminUser = await _userService.GetUserByEmailOrUserNameAsync(User.Identity.Name);
                    if (adminUser != null)
                    {
                        await _activityService.LogAdminAccessAsync(adminUser.Id, "Accessed System Settings", HttpContext);
                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing system settings");
                TempData["Error"] = "Error loading system settings.";
                return RedirectToAction("Index");
            }
        }

        // TRANSLATOR METHODS - Updated to match wireframe
        [HttpGet]
        public async Task<IActionResult> Translator()
        {
            try
            {
                // Log admin access to translator
                if (_activityService != null && User.Identity?.Name != null)
                {
                    var adminUser = await _userService.GetUserByEmailOrUserNameAsync(User.Identity.Name);
                    if (adminUser != null)
                    {
                        await _activityService.LogAdminAccessAsync(adminUser.Id, "Accessed Subtitle Translator", HttpContext);
                    }
                }

                ViewData["Title"] = "AI Translator - CineHub Admin";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing translator");
                return RedirectToAction("Index", "Admin");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadMovieFile(IFormFile movieFile)
        {
            try
            {
                if (movieFile == null || movieFile.Length == 0)
                {
                    return Json(new { success = false, message = "No file selected" });
                }

                // Validate file type
                var allowedExtensions = new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv" };
                var fileExtension = Path.GetExtension(movieFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Invalid file type. Please upload a video file." });
                }

                // Check file size (limit to 2GB)
                if (movieFile.Length > 2147483648) // 2GB
                {
                    return Json(new { success = false, message = "File size too large. Maximum size is 2GB." });
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "translator");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await movieFile.CopyToAsync(stream);
                }

                // Log activity
                if (_activityService != null && User.Identity?.Name != null)
                {
                    var adminUser = await _userService.GetUserByEmailOrUserNameAsync(User.Identity.Name);
                    if (adminUser != null)
                    {
                        await _activityService.LogActivityAsync(
                            adminUser.Id,
                            "Movie Upload",
                            $"Uploaded movie file for translation: {movieFile.FileName}"
                        );
                    }
                }

                return Json(new
                {
                    success = true,
                    message = "Movie file uploaded successfully",
                    fileName = movieFile.FileName,
                    filePath = $"/uploads/translator/{fileName}",
                    fileSize = movieFile.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading movie file for translation");
                return Json(new { success = false, message = "An error occurred while uploading the file" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadSubtitleFile(IFormFile subtitleFile)
        {
            try
            {
                if (subtitleFile == null || subtitleFile.Length == 0)
                {
                    return Json(new { success = false, message = "No subtitle file selected" });
                }

                // Validate file type
                var allowedExtensions = new[] { ".srt", ".vtt", ".ass", ".ssa", ".sub" };
                var fileExtension = Path.GetExtension(subtitleFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Invalid file type. Please upload a subtitle file (.srt, .vtt, .ass, .ssa, .sub)." });
                }

                // Read file content
                string content;
                using (var reader = new StreamReader(subtitleFile.OpenReadStream(), Encoding.UTF8))
                {
                    content = await reader.ReadToEndAsync();
                }

                // Log activity
                if (_activityService != null && User.Identity?.Name != null)
                {
                    var adminUser = await _userService.GetUserByEmailOrUserNameAsync(User.Identity.Name);
                    if (adminUser != null)
                    {
                        await _activityService.LogActivityAsync(
                            adminUser.Id,
                            "Subtitle Upload",
                            $"Uploaded subtitle file for translation: {subtitleFile.FileName}"
                        );
                    }
                }

                return Json(new
                {
                    success = true,
                    message = "Subtitle file uploaded successfully",
                    fileName = subtitleFile.FileName,
                    content = content
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading subtitle file for translation");
                return Json(new { success = false, message = "An error occurred while uploading the subtitle file" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TranslateSubtitle([FromBody] TranslateRequest request)
        {
            try
            {
                // Simulate translation process (replace with actual translation API)
                await Task.Delay(2000); // Simulate processing time

                // Log translation activity
                if (_activityService != null && User.Identity?.Name != null)
                {
                    var adminUser = await _userService.GetUserByEmailOrUserNameAsync(User.Identity.Name);
                    if (adminUser != null)
                    {
                        await _activityService.LogActivityAsync(
                            adminUser.Id,
                            "Subtitle Translation",
                            $"Translated subtitle from {request.SourceLanguage} to {request.TargetLanguage}"
                        );
                    }
                }

                // Mock translation result
                var translatedText = SimulateTranslation(request.OriginalText, request.SourceLanguage, request.TargetLanguage);

                return Json(new
                {
                    success = true,
                    translatedText = translatedText,
                    sourceLanguage = request.SourceLanguage,
                    targetLanguage = request.TargetLanguage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating subtitle");
                return Json(new { success = false, message = "Translation failed" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveTranslatorNote([FromBody] SaveNoteRequest request)
        {
            try
            {
                // Log note saving activity
                if (_activityService != null && User.Identity?.Name != null)
                {
                    var adminUser = await _userService.GetUserByEmailOrUserNameAsync(User.Identity.Name);
                    if (adminUser != null)
                    {
                        await _activityService.LogActivityAsync(
                            adminUser.Id,
                            "Translation Note",
                            "Saved translation note"
                        );
                    }
                }

                return Json(new { success = true, message = "Note saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving translator note");
                return Json(new { success = false, message = "Failed to save note" });
            }
        }

        // Analytics - Simplified version
        public async Task<IActionResult> Analytics()
        {
            try
            {
                // Log admin access
                if (_activityService != null && User.Identity?.Name != null)
                {
                    var adminUser = await _userService.GetUserByEmailOrUserNameAsync(User.Identity.Name);
                    if (adminUser != null)
                    {
                        await _activityService.LogAdminAccessAsync(adminUser.Id, "Accessed Analytics", HttpContext);
                    }
                }

                // Get basic statistics using existing methods
                var allUsers = await _userService.GetAllUsersAsync();
                var allMovies = await _movieService.GetAllMoviesAsync();

                ViewBag.TotalUsers = allUsers.Count;
                ViewBag.TotalMovies = allMovies.Count();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics");
                TempData["Error"] = "Error loading analytics.";
                return RedirectToAction("Index");
            }
        }

        // API Methods for AJAX calls
        [HttpGet]
        public async Task<IActionResult> GetUserData(string userId)
        {
            try
            {
                // Find user by ID from all users
                var allUsers = await _userService.GetAllUsersAsync();
                var user = allUsers.FirstOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var roles = await _userService.GetUserRolesAsync(user);

                return Json(new
                {
                    success = true,
                    user = new
                    {
                        id = user.Id,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        email = user.Email,
                        userName = user.UserName,
                        displayUserName = user.DisplayUserName,
                        isActive = user.IsActive,
                        createdAt = user.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user data for {userId}");
                return Json(new { success = false, message = "Error retrieving user data" });
            }
        }

        // Helper method for translation simulation
        private string SimulateTranslation(string originalText, string sourceLanguage, string targetLanguage)
        {
            // This is a mock translation. In a real application, you would call a translation API
            var lines = originalText.Split('\n');
            var translatedLines = new List<string>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    translatedLines.Add(line);
                    continue;
                }

                // Keep timestamps and sequence numbers as is
                if (line.Contains("-->") || line.All(char.IsDigit))
                {
                    translatedLines.Add(line);
                    continue;
                }

                // Mock translation based on target language
                var translatedLine = targetLanguage switch
                {
                    "vi" => $"[VI] {line}",
                    "ja" => $"[JP] {line}",
                    "ko" => $"[KO] {line}",
                    "zh" => $"[ZH] {line}",
                    "es" => $"[ES] {line}",
                    "fr" => $"[FR] {line}",
                    "de" => $"[DE] {line}",
                    _ => $"[EN] {line}"
                };

                translatedLines.Add(translatedLine);
            }

            return string.Join('\n', translatedLines);
        }

        // Error handling
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // Request models for the translator
    public class TranslateRequest
    {
        public string OriginalText { get; set; } = "";
        public string SourceLanguage { get; set; } = "";
        public string TargetLanguage { get; set; } = "";
    }

    public class SaveNoteRequest
    {
        public string Note { get; set; } = "";
    }
}