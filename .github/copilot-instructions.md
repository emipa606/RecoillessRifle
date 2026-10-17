# GitHub Copilot Instructions for Recoilless Rifle (Continued) Mod

## Mod Overview and Purpose
**Recoilless Rifle (Continued)** is an updated version of match_milk's original mod, designed to enhance the RimWorld experience by adding a versatile recoilless rifle turret. This mod allows players to engage in strategic defense and offense by offering a variety of ammunition types and tactical scenarios.

## Key Features and Systems
- **Recoilless Rifle Turret**: A buildable turret available after researching mortars. It provides a direct fire option with unique shell types.
  - **Attributes**:
    - Must be manned to operate.
    - Cannot shoot over walls.
    - Features low muzzle velocity.
    - Construction cost: 100 Steel, 3 Components.

- **Ammunition Types**:
  - High-Explosive (HE) Shell: Standard explosive projectile.
  - Cluster Bombs: Multiple bomblets for wide area impact.
  - Incendiary Cluster Bomb: Several small bombs for spreading fire.
  - HEAT (High-Explosive Anti-Tank): Extra damage on direct impact.
  - Shotgun Shell: Spreads 100 pellets over long distances.
  - Canister Shot: Uniform pellet dispersion at any range.
  - Smoke Shell: Generates smoke on impact, reducing hit probability through smoke.
  - EMP Shell: Disables electronics within the impact radius.
  - Armor Piercing Shell: Simple shell for penetrating armor.
  - Sabot Round: Enhanced armor penetration capabilities.

- **Compatibility**:
  - Compatible with most mods and existing saves.
  - Not compatible with "Combat Extended."

## Coding Patterns and Conventions
- **C# Structure**: The mod uses a clear and organized structure with separate files for each component and functionality.
- **Naming Conventions**: Consistent prefixing (e.g., `RR_` for XML definitions) and CamelCase for C# class and method names.
- **Modular Design**: Separate classes for turret behavior (`Building_TurretGunCustom`), projectiles (`Projectile_Custom`), and turret components (`CompTurretTopSize`).

## XML Integration
- **Defs Configuration**: XML files define objects, categories, and properties that are used in-game.
  - Example files include `RR_ThingCategories.xml` for categorizing items and `RR_Buildings_RecoillessRifle.xml` for defining the turret and its construction requirements.

## Harmony Patching
- The mod does not specify explicit Harmony patches, suggesting direct integration into the game's existing systems without overriding core functionalities. It's crucial to maintain compatibility by avoiding invasive patches unless necessary.

## Suggestions for Copilot
- **Autocomplete Assistance**: Utilize Copilot for completing C# method bodies by generating common RimWorld modding patterns like `Tick()`, `SpawnSetup()`, and `DrawAt()`.
- **Code Error Prediction**: Use Copilot's capabilities to predict and resolve potential coding errors, especially around new feature integration.
- **XML Definitions**: Leverage Copilot to auto-generate XML entry templates to ensure they conform to RimWorld's XML schema.
- **Documentation**: Encourage Copilot to suggest comments for methods and classes, enhancing code readability and maintainability, especially for public mods.

By following these guidelines, development and maintenance of the **Recoilless Rifle (Continued)** mod can be streamlined, contributing to a balanced and immersive RimWorld experience.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

