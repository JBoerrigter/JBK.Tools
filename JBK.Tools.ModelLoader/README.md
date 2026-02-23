# Loader Export + glTF Validation

## Export a `.gb` to `.glb`

From repo root:

```powershell
dotnet run --project JBK.Tools.ModelLoader/JBK.Tools.ModelLoader.csproj -- --filename TestAssets/lamp.gb --export glb --out TestAssets/lamp.glb
```

Optional diagnostics (chunk/accessor/skin/animation checks):

```powershell
dotnet run --project JBK.Tools.ModelLoader/JBK.Tools.ModelLoader.csproj -- --filename TestAssets/lamp.gb --export glb --out TestAssets/lamp.glb --export-diagnostics
```

The exporter now runs strict internal conformance checks after writing GLB and fails on hard errors.

## Run official Khronos validator

Install Node dependencies once:

```powershell
cd JBK.Tools.ModelLoader
npm install
```

Validate one or more GLBs:

```powershell
node ./scripts/validate-glb.js ../TestAssets/lamp.glb
```

Run export + validation in one step:

```powershell
npm run validate:lamp
```

`validate-glb.js` exits non-zero when validator errors are found.

## Publish Release single-file

From repo root:

```powershell
dotnet publish JBK.Tools.ModelLoader/JBK.Tools.ModelLoader.csproj -p:PublishProfile=win-x64-singlefile
```

Output:

`JBK.Tools.ModelLoader/bin/Release/net9.0/win-x64/publish/JBK.Tools.ModelLoader.exe`

Smaller but riskier (trimming can break runtime behavior in reflection-heavy libraries):

```powershell
dotnet publish JBK.Tools.ModelLoader/JBK.Tools.ModelLoader.csproj -p:PublishProfile=win-x64-singlefile-trimmed
```
