using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Stark_induestries.Entity;

namespace Stark_induestries.Service;

public class CloudService
{
    
    private readonly Cloudinary _cloudinary;
    private readonly string _cloudName ;
    
    //TODO check if this works right for every image, otherwise calculate it
    //Height and width variables
    private readonly int width_s = 720;
    private readonly int height_s = 520;
    private readonly int width_m = 1020;
    private readonly int height_m = 820;
    private readonly int width_b = 1920;
    private readonly int height_b = 1820;
 
    public CloudService(Cloudinary cloudinary, IConfiguration config)
    {
        _cloudinary = cloudinary;
        _cloudName  = config["Cloudinary:CLOUD_NAME"]!;
    }
 
    public async Task<String> UploadImage (IFormFile file)
    {
        
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var baseName = Path.GetFileNameWithoutExtension(file.FileName);
        
        
        //Bulk uploads all the variants
        await Task.WhenAll(
            UploadVariant(ms,baseName,null,null,baseName),
            UploadVariant(ms, baseName,width_s,height_s,$"{baseName}_s"),
            UploadVariant(ms, baseName,width_m,height_m,$"{baseName}_m"),
            UploadVariant(ms, baseName,width_b,height_b,$"{baseName}_b")
        );
        
        return baseName;
    }
    
    private async Task UploadVariant(MemoryStream ms, string fileName, int? width, int? height, string publicId)
    {
        var transformation = (width.HasValue && height.HasValue) ? new Transformation().Width(width).Height(height).Crop("scale") : null;
 
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, new MemoryStream(ms.ToArray())),
            PublicId  = publicId,
            Overwrite = false,
            Transformation = transformation
        };
 
        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            throw new Exception($"Cloudinary upload failed for {publicId}: {result.Error.Message}");
        }
    }
    
    public async Task<string> GetImageUrl (string publicId, string size)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
 

        var result = await _cloudinary.GetResourceAsync(new GetResourceParams(publicId));
        
        if (result.Error != null)
        {
            throw new Exception($"Cloudinary fetch failed: {result.Error.Message}");
        }

        var suffixedId = size == "original" ? publicId : $"{publicId}_{size}";
        var url = $"https://res.cloudinary.com/{_cloudName}/image/upload/{suffixedId}";
        
        return url;
    }
    
    public async Task<IEnumerable<string>> GetBasePublicIds()
    {
        var result = await _cloudinary.Search().Expression("resource_type:image").SortBy("created_at", "asc").MaxResults(10).ExecuteAsync();
 
        return result.Resources
            .Select(r => r.PublicId)
            //Filtering out results with the base image
            .Where(id => !id.EndsWith("_s") && !id.EndsWith("_m") && !id.EndsWith("_b"));
    }


    
    
    
    public async Task Delete(string publicId)
    {
        await Task.WhenAll(
            _cloudinary.DestroyAsync(new DeletionParams(publicId)),
            _cloudinary.DestroyAsync(new DeletionParams($"{publicId}_s")),
            _cloudinary.DestroyAsync(new DeletionParams($"{publicId}_m")),
            _cloudinary.DestroyAsync(new DeletionParams($"{publicId}_b"))
        );
    }
    

    
    
    
}