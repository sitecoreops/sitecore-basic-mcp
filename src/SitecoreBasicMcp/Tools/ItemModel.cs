namespace SitecoreBasicMcp.Tools;

public record Field(string Name, string? Value);
public record Fields(Field[] Nodes);
public record ChildItem(string Id, string Name, string Path, int Version);
public record Children(ChildItem[] Nodes);
public record Template(string Id, string Name, string FullName);
public record Parent(string Id, string Name, string Path, int Version);

public class BasicItem
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Path { get; set; }
    public int Version { get; set; }
}

public class Item : BasicItem
{
    public required Fields Fields { get; set; }
    public required Children Children { get; set; }
    public required Template Template { get; set; }
    public Parent? Parent { get; set; }

}