namespace core.Interfaces;

public interface IPhysicalFileWriterFactory
{

    IPhysicalFileWriter Create(string filePath, long preallocationSize);
}
