# ThirdParty Svelto

This package is the staging location for Svelto ECS source used by AbilityKit.

Recommended layout after downloading upstream source:

```text
Unity/Packages/com.abilitykit.thirdparty.svelto/Runtime/Svelto.Common/
Unity/Packages/com.abilitykit.thirdparty.svelto/Runtime/Svelto.ECS/
Unity/Packages/com.abilitykit.thirdparty.svelto/Runtime/Svelto.Tasks/
```

Keep Unity-only editor or debug tooling outside Runtime, or place it under Editor so server-side .NET projects can exclude it cleanly.

The matching .NET project is:

```text
src/AbilityKit.ThirdParty.Svelto/AbilityKit.ThirdParty.Svelto.csproj
```

After the source is copied in, adjust the project Include/Exclude patterns only if the upstream layout differs from the recommended layout.
