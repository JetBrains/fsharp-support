using JetBrains.ReSharper.Plugins.FSharp.Checker;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.Threading;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Resolve
{
  public class FSharpModuleResolvedSymbols : IFSharpModuleResolvedSymbols
  {
    private readonly FcsFileResolvedSymbols[] myFileResolvedSymbols;

    public IPsiModule PsiModule { get; }
    public FcsCheckerService CheckerService { get; }
    public IFcsProjectProvider FcsProjectProvider { get; }

    private readonly JetFastSemiReenterableRWLock myLock = new JetFastSemiReenterableRWLock();

    public FSharpModuleResolvedSymbols(IPsiModule psiModule, int filesCount, FcsCheckerService checkerService,
      IFcsProjectProvider fcsProjectProvider)
    {
      myFileResolvedSymbols = new FcsFileResolvedSymbols[filesCount];

      PsiModule = psiModule;
      CheckerService = checkerService;
      FcsProjectProvider = fcsProjectProvider;
    }

    public void Invalidate(IPsiSourceFile sourceFile)
    {
      using (myLock.UsingWriteLock())
      {
        var fileIndex = FcsProjectProvider.GetFileIndex(sourceFile);
        if (fileIndex == -1)
          return;

        var filesCount = myFileResolvedSymbols.Length;
        for (var i = fileIndex; i < filesCount; i++)
          myFileResolvedSymbols[i] = null;
      }
    }

    private FcsFileResolvedSymbols TryGetResolvedSymbols(int fileIndex)
    {
      using (myLock.UsingReadLock())
        return myFileResolvedSymbols[fileIndex];
    }

    public IFcsFileResolvedSymbols GetResolvedSymbols(IPsiSourceFile sourceFile)
    {
      var fileIndex = FcsProjectProvider.GetFileIndex(sourceFile);
      var fileResolvedSymbols = TryGetResolvedSymbols(fileIndex);
      if (fileResolvedSymbols != null)
        return fileResolvedSymbols;

      using (myLock.UsingWriteLock())
      {
        fileResolvedSymbols = TryGetResolvedSymbols(fileIndex);
        if (fileResolvedSymbols != null)
          return fileResolvedSymbols;

        fileResolvedSymbols = new FcsFileResolvedSymbols(sourceFile, CheckerService);
        myFileResolvedSymbols[fileIndex] = fileResolvedSymbols;
      }

      return fileResolvedSymbols;
    }

    public void Invalidate()
    {
      using (myLock.UsingWriteLock())
        for (var i = 0; i < myFileResolvedSymbols.Length; i++)
          myFileResolvedSymbols[i] = null;
    }
  }
}
