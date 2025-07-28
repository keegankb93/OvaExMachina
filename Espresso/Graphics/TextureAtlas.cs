using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Espresso.Graphics;

public class TextureAtlas
{
    private record RegionData(string Name, string Group, string Frame, int X, int Y, int Width, int Height);
    private Dictionary<string, TextureRegion> _regions;
    
    
    private record AnimationData(string Name, string Group, string Frames, System.TimeSpan Delay);
    // Stores animations added to this atlas.
    private Dictionary<string, Animation> _animations;

    /// <summary>
    /// Gets or Sets the source texture represented by this texture atlas.
    /// </summary>
    public Texture2D Texture { get; set; }
    
    /// <summary>
    /// Creates a new texture atlas.
    /// </summary>
    public TextureAtlas()
    {
        _regions = new Dictionary<string, TextureRegion>();
        _animations = new Dictionary<string, Animation>();
    }

    /// <summary>
    /// Creates a new texture atlas instance using the given texture.
    /// </summary>
    /// <param name="texture">The source texture represented by the texture atlas.</param>
    public TextureAtlas(Texture2D texture)
    {
        Texture = texture;
        _regions = new Dictionary<string, TextureRegion>();
        _animations = new Dictionary<string, Animation>();
    }
    
    /// <summary>
    /// Creates a new region and adds it to this texture atlas.
    /// </summary>
    /// <param name="name">The name to give the texture region.</param>
    /// <param name="x">The top-left x-coordinate position of the region boundary relative to the top-left corner of the source texture boundary.</param>
    /// <param name="y">The top-left y-coordinate position of the region boundary relative to the top-left corner of the source texture boundary.</param>
    /// <param name="width">The width, in pixels, of the region.</param>
    /// <param name="height">The height, in pixels, of the region.</param>
    public void AddRegion(string name, int x, int y, int width, int height)
    {
        var region = new TextureRegion(Texture, x, y, width, height);
        _regions.Add(name, region);
    }

    /// <summary>
    /// Gets the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="name">The name of the region to retrieve.</param>
    /// <returns>The TextureRegion with the specified name.</returns>
    public TextureRegion GetRegion(string name)
    {
        return _regions[name];
    }

    /// <summary>
    /// Removes the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="name">The name of the region to remove.</param>
    /// <returns></returns>
    public bool RemoveRegion(string name)
    {
        return _regions.Remove(name);
    }

    /// <summary>
    /// Removes all regions from this texture atlas.
    /// </summary>
    public void Clear()
    {
        _regions.Clear();
    }

    /// <summary>
    /// Creates a new texture atlas based a texture atlas xml configuration file.
    /// </summary>
    /// <param name="content">The content manager used to load the texture for the atlas.</param>
    /// <param name="fileName">The path to the xml file, relative to the content root directory.</param>
    /// <returns>The texture atlas created by this method.</returns>
    public static TextureAtlas FromFile(ContentManager content, string fileName)
    {
        var atlas = new TextureAtlas();
        var filePath = Path.Combine(content.RootDirectory, fileName);

        using var stream = TitleContainer.OpenStream(filePath);
        using var reader = XmlReader.Create(stream);
        
        var doc = XDocument.Load(reader);
        var root = doc.Root;

        // The <Texture> element contains the content path for the Texture2D to load.
        // So we will retrieve that value then use the content manager to load the texture.
        var resource = root?.Element("Resource")?.Value ?? throw new XmlException("Missing <Resource> element in the XML.");

        var texturePath = root?.Element("Texture")?.Value ?? throw new XmlException("Missing <Texture> element in the XML.");
        
        atlas.Texture = content.Load<Texture2D>(texturePath);

        // The <Regions> element contains individual <Region> elements, each one describing
        // a different texture region within the atlas.  
        //
        // Example:
        // <Regions>
        //      <Region name="spriteOne" x="0" y="0" width="32" height="32" />
        //      <Region name="spriteTwo" x="32" y="0" width="32" height="32" />
        // </Regions>
        //
        // So we retrieve all of the <Region> elements then loop through each one
        // and generate a new TextureRegion instance from it and add it to this atlas.
        var regions = root.Element("Regions")?.Elements("Region") ?? throw new XmlException("Missing <Regions> element in the XML.");
        
        AddRegions(resource, atlas, regions);
        
        // The <Animations> element contains individual <Animation> elements, each one describing
        // a different animation within the atlas.
        //
        // Example:
        // <Animations>
        //      <Animation name="animation" delay="100">
        //          <Frame region="spriteOne" />
        //          <Frame region="spriteTwo" />
        //      </Animation>
        // </Animations>
        //
        // So we retrieve all of the <Animation> elements then loop through each one
        // and generate a new Animation instance from it and add it to this atlas.
        var animationElements = root.Element("Animations")?.Elements("Animation")  ?? throw new XmlException("Missing <Animations> element in the XML.");
        
        AddAnimations(resource, atlas, animationElements);
        
        return atlas;
    }
    
    private static void AddRegions(string resource, TextureAtlas atlas, IEnumerable<XElement> regions)
    {
        foreach (var region in regions)
        {
            var data = ParseRegionElement(region);
            var id = $"{resource}:{data.Group}:{data.Name}:{data.Frame}";
            
            atlas.AddRegion(id, data.X, data.Y, data.Width, data.Height);
        }
    }

    private static RegionData ParseRegionElement(XElement regionElement)
    {
        var name = regionElement.Attribute("name")?.Value 
                   ?? throw new Exception("Missing 'name' attribute in <Region>");

        var group = regionElement.Attribute("group")?.Value 
                    ?? throw new Exception("Missing 'group' attribute in <Region>");

        var frame = regionElement.Attribute("frame")?.Value 
                    ?? throw new Exception("Missing 'frame' attribute in <Region>");
            
        var x = int.Parse(regionElement.Attribute("x")?.Value ?? "0");
        var y = int.Parse(regionElement.Attribute("y")?.Value ?? "0");
        var width = int.Parse(regionElement.Attribute("width")?.Value ?? "0");
        var height = int.Parse(regionElement.Attribute("height")?.Value ?? "0");
        
        return new RegionData(name, group, frame, x, y, width, height);
    }

    private static void AddAnimations(string resource, TextureAtlas atlas, IEnumerable<XElement> animationElements)
    {
        foreach (var animationElement in animationElements)
        {
            var data = ParseAnimationElement(animationElement);
            var id = $"{resource}:{data.Group}:{data.Name}";
            var selectedFrames = SelectFrames(id, atlas, data.Frames);
            var animation = new Animation(selectedFrames, data.Delay);
            
            atlas.AddAnimation(id, animation);
        }
    }

    private static AnimationData ParseAnimationElement(XElement animationElement)
    {
        var name = animationElement.Attribute("name")?.Value 
                   ?? throw new Exception("Missing 'name' attribute in <Animation>");

        var group = animationElement.Attribute("group")?.Value 
                    ?? throw new Exception("Missing 'group' attribute in <Animation>");        
        
        var frames  = animationElement.Attribute("frames")?.Value ?? throw new Exception("Missing 'frames' attribute in <Animation>");
        
        var delayInMilliseconds = float.Parse(animationElement.Attribute("delay")?.Value ?? "0");
        var delay = TimeSpan.FromMilliseconds(delayInMilliseconds);

        return new AnimationData(name, group, frames, delay);
    }

    private static List<TextureRegion> SelectFrames(string animationId, TextureAtlas atlas, string frameRange)
    {
        var frames = new List<TextureRegion>();
        
        // TODO: We can make sure that the range is formatted and valid + add a fallback option if frames is not a range to use the <Frame /> pattern
        // I.e. we should make sure length is good, no whitespace/valid format etc.
        var range = frameRange.Split("-");
            
        frames.AddRange(range.Select(frameElement => atlas.GetRegion($"{animationId}:{frameElement}")));

        return frames;
    }

    /// <summary>
    /// Creates a new sprite using the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="regionName">The name of the region to create the sprite with.</param>
    /// <returns>A new Sprite using the texture region with the specified name.</returns>
    public Sprite CreateSprite(string regionName)
    {
        var region = GetRegion(regionName);
        return new Sprite(region);
    }
    
    /// <summary>
    /// Adds the given animation to this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to add.</param>
    /// <param name="animation">The animation to add.</param>
    public void AddAnimation(string animationName, Animation animation)
    {
        _animations.Add(animationName, animation);
    }

    /// <summary>
    /// Gets the animation from this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to retrieve.</param>
    /// <returns>The animation with the specified name.</returns>
    public Animation GetAnimation(string animationName)
    {
        return _animations[animationName];
    }

    /// <summary>
    /// Removes the animation with the specified name from this texture atlas.
    /// </summary>
    /// <param name="animationName">The name of the animation to remove.</param>
    /// <returns>true if the animation is removed successfully; otherwise, false.</returns>
    public bool RemoveAnimation(string animationName)
    {
        return _animations.Remove(animationName);
    }
    
    /// <summary>
    /// Creates a new animated sprite using the animation from this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to use.</param>
    /// <returns>A new AnimatedSprite using the animation with the specified name.</returns>
    public AnimatedSprite CreateAnimatedSprite(string animationName)
    {
        var animation = GetAnimation(animationName);
        return new AnimatedSprite(animation);
    }
}