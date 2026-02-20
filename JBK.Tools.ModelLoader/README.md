# Loader Export + glTF Validation

## Export a `.gb` to `.glb`

From repo root:

```powershell
dotnet run --project Loader/JBK.Tools.ModelLoader.csproj -- --filename TestAssets/lamp.gb --export glb --out TestAssets/lamp.glb
```

Optional diagnostics (chunk/accessor/skin/animation checks):

```powershell
dotnet run --project Loader/JBK.Tools.ModelLoader.csproj -- --filename TestAssets/lamp.gb --export glb --out TestAssets/lamp.glb --export-diagnostics
```

The exporter now runs strict internal conformance checks after writing GLB and fails on hard errors.

## Run official Khronos validator

Install Node dependencies once:

```powershell
cd Loader
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
