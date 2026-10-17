# GitHub Copilot Instructions for RimWorld Mod: Hidden Enemy Bio (Continued)

## Mod Overview and Purpose

**Mod Name**: Hidden Enemy Bio (Continued)  
**Author**: Mlie (originally by cjgunnar)  
**Package ID**: Mlie.HiddenEnemyBio

The Hidden Enemy Bio mod provides an immersive experience by concealing the biographies of enemies and prisoners until their resistance decreases to specific thresholds. By hiding details such as skills, backstories, and traits, this mod adds depth to the decision-making process regarding which captured enemies to hold and which to release.

## Key Features and Systems

- **Dynamic Bio Revealing**: The mod gradually reveals prisoner bios based on their resistance levels:
  - Skills they are passionate about at resistance 18.
  - Backstory (childhood and adulthood) at resistance 18.
  - Traits at resistance 15.
  - All skills at resistance 10.
- **Selective Revealing**: Traits modified by genes are always visible. No effect on friendly characters, slaves, or colonists.
- **Configurable Settings**: Users can adjust the resistance thresholds in the settings to customize their gameplay experience.
- **Compatibility**: Works with Vanilla Skills Expanded and 1-2-3 Personalities mods, albeit with minor adjustments for optimal experience.

## Coding Patterns and Conventions

### General Coding Practices

- Use descriptive names for classes, methods, and variables to enhance code readability.
- Consistently follow the C# naming conventions, using PascalCase for classes and methods and camelCase for variables.
- Utilize regions in C# code to organize logical sections and keep the codebase manageable.

### File Structure

- Each class is contained within its own C# file for clarity and separation of concerns.
- Class files have clear purposes, with members organized in logical groupings.

## XML Integration

Integrating XML is straightforward in this mod, with configurations predominantly handled in the `About.xml` file. Ensure all necessary dependencies and mod information are properly defined at this location, specifically noting references to external libraries or tools, like Harmony.

## Harmony Patching

- **Purpose**: Harmony is used to patch the game's original code, allowing the mod to subtly alter how certain functions operate without directly modifying the game's source.
- **Implementation**: Key methods such as `Prefix` are used in harmony patches, preparing the environment for mod-specific logic:
  - Files like `SkillUI_DrawSkillsOf.cs` and `CharacterCardUtility_DrawCharacterCard.cs` contain essential Harmony patches.

## Suggestions for Copilot

- **Refactor Codes**: Use Copilot to generate skeletons for new feature classes, adhering to existing patterns.
- **Test Coverage**: Encourage Copilot to suggest test cases around new and existing features, ensuring stability with every deployment.
- **Localization**: Use Copilot to manage localization threads, aiding with keeping translations synchronized across different locales.
- **Enhanced Readability**: Leverage Copilot to produce documentation comments that improve code comprehension for complex methods or classes.

---

Ensure to always consider compatibilities and interactions with other mods when implementing changes or introducing new features.

This mod is licensed under the MIT License, allowing community contributions and modifications with attribution to the original creators.

For additional support, feedback, or issue reporting, please use GitHub's issues page or the designated Discord channel.


This copilot instruction file is structured to guide developers using GitHub Copilot to understand and extend the Hidden Enemy Bio mod efficiently. It includes high-level details and specific coding strategies reflecting the existing project's architecture.

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

