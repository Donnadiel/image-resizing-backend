
using System.Text.Json;
using Stark_induestries.Entity;

namespace Stark_induestries.Service;

public class ImageQueueService
{
    private readonly LinkedList<ImageEntry> _imageQueue = [];
    private const int MaxSize = 10;
    
    public async Task Enqueue(ImageEntry entry, CloudService cloud)
    {
        if (_imageQueue.Count >= MaxSize)
        {
            var oldest = _imageQueue.First!.Value;
            _imageQueue.RemoveFirst();
            await cloud.Delete(oldest.Identifier);
        }
 
        _imageQueue.AddLast(entry);
 
        //Readjust indexes
        var i = 0;
        foreach (var image in _imageQueue)
            image.Index = i++;
    }

    public ImageEntry? GetByIndex(int index)
    {
        return _imageQueue.FirstOrDefault(entry => entry.Index == index);
    }
        
    
    
}
