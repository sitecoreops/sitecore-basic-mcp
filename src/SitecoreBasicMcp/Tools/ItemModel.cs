namespace SitecoreBasicMcp.Tools;

public record Field(string Name, string? Value);
public record Fields(Field[] Nodes);
public record ChildItem(string Id, string Name, string Path);
public record Children(ChildItem[] Nodes);
public record Template(string Id, string Name, string FullName);
public record Parent(string Id, string Name, string Path);
public record Item(string Id, string Name, string Path, Fields Fields, Children Children, Template Template, Parent? Parent);
public record BasicItem(string Id, string Name, string Path);
