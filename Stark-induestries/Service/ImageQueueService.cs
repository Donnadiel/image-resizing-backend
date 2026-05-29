
using System.Text.Json;
using Stark_induestries.Entity;

namespace Stark_induestries.Service;

public class ImageQueueService
{
    private readonly LinkedList<ImageEntry> _imageQueue = [];
    private const int MaxSize = 10;
    private const string QueuePath = "Storage/queue.json";

    public async Task Enqueue(ImageEntry entry, CloudService cloud)
    {

        if (_imageQueue.Count >= MaxSize)
        {
            var oldest = _imageQueue.First!.Value;
            _imageQueue.RemoveFirst();
            await cloud.Delete(oldest.Identifier);
        }
        
        entry.Index = _imageQueue.Count;
        _imageQueue.AddLast(entry);
        
        //Readjust indexes
        var i = 0;
        foreach (var image in _imageQueue)
        {
            image.Index = i++;
        }

        await SaveQueue();

    }

    public ImageEntry? GetByIndex(int index)
    {
        var reg = _imageQueue.FirstOrDefault(entry => entry.Index == index);
        return reg;
    }
    
    private async Task SaveQueue()
    {
        Directory.CreateDirectory("Storage");
        var ids = _imageQueue.Select(e => e.Identifier).ToList();
        await File.WriteAllTextAsync(QueuePath, JsonSerializer.Serialize(ids));
    }
    
    //Dynamically reload saved Queue 
    public async Task ReloadQueue()
    {
        if (!File.Exists(QueuePath)) return;

        var json = await File.ReadAllTextAsync(QueuePath);
        var ids = JsonSerializer.Deserialize<List<string>>(json);
    
        if (ids == null) return;

        foreach (var id in ids)
        {
            var entry = new ImageEntry { Identifier = id, Index = _imageQueue.Count };
            _imageQueue.AddLast(entry);
        }
    }

    
    
    

}