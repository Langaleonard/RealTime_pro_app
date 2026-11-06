using ABCRetail.Models;
using ABCRetail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class ProductsController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorageService _blobStorageService;

        public ProductsController(
            TableStorageService tableStorageService,
            BlobStorageService blobStorageService)
        {
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
        }

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            var products =
                await _tableStorageService.GetProductsAsync();

            return View(products);
        }

        // GET: /Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Product product,
            IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError(
                    "ImageName",
                    "Please select a product image.");
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            string imageName =
                await _blobStorageService.UploadImageAsync(imageFile!);

            product.PartitionKey = "Product";
            product.RowKey = Guid.NewGuid().ToString();
            product.ImageName = imageName;

            await _tableStorageService.AddProductAsync(product);

            return RedirectToAction(nameof(Index));
        }

        // Displays private Azure Blob images through the web app
        public async Task<IActionResult> Image(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return NotFound();
            }

            try
            {
                var image =
                    await _blobStorageService
                        .DownloadImageAsync(blobName);

                return File(
                    image.Content,
                    image.ContentType);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}