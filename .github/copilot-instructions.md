# Copilot Instructions for RimWorld Modding in C#

## Mod Overview
**Mod Name:** Hidden Enemy Bio (Continued)

This mod was originally created by cjgunnars and has been updated to include additional tooltip information for hidden attributes. It features Chinese and Russian translations by BUyue and Osetrokarasek, respectively.

**Purpose:** 
Hidden Enemy Bio offers a more realistic and immersive experience by concealing the biographies of enemies and prisoners in the game until certain conditions are met, such as lowering their resistance. The mod adds strategic depth to the decision-making process regarding which captured enemies to keep, as information is revealed gradually.

## Key Features and Systems

- **Concealed Information:** Hides enemy and prisoner biographies until their resistance drops to specified levels.
- **Configurable Settings:** Customize thresholds for revealing different aspects of biographical data, such as skills, traits, and backstories.
- **Exclusions:** The mod does not affect the biographies of friendly characters, slaves, or colonists.
- **Translations:** Available in Chinese and Russian languages.
- **Compatibility:** 
  - Does not affect game saves and can be added or removed safely.
  - Compatible with certain mods like RimHUD and Vanilla Skills Expanded, but may be incompatible with mods that modify the Bio tab, like Stranger Danger and Well Met.

## Coding Patterns and Conventions

- **Project Structure:** Follows a typical C# modding structure, with separate classes for different functionalities.
- **Coding Style:** Follows C# conventions for naming and accessing properties. Uses public, internal, and private access modifiers to encapsulate functionality appropriately.

## XML Integration

The mod integrates XML files for easy configuration and localization:
- **XML Settings** allow users to define resistance thresholds for revealing attributes.
- **Localization Files** support multiple languages, enhancing accessibility for non-English speakers.

## Harmony Patching

The mod utilizes the Harmony library for non-destructive patching:
- **Harmony Patch Types:** Uses prefix, postfix, and transpiler patches to alter the base game's functionality related to the Bio tab without modifying the original code directly.
- **Targeted Patches:** Patches are applied to methods involved in drawing character cards and skill UI, e.g., `CharacterCardUtility_DrawCharacterCard` and `SkillUI_DrawSkillsOf`.

## Suggestions for Copilot

- **Assist with Harmony Patches:** Generate templates for common Harmony patching methods, suggesting where to insert prefix and postfix logic.
- **XML Modification:** Recommend snippets for XML settings files, including examples for defining configurable properties.
- **Localization Support:** Provide suggestions for additional translations or updates to existing localization files.
- **Error Handling:** Suggest best practices for error handling in mods, such as validating settings and providing user feedback.
- **Code Convention Enforcement:** Offer tips to ensure consistent adherence to C# conventions across the project.

By following these instructions, developers can efficiently maintain and expand the Hidden Enemy Bio mod, ensuring it stays functional and relevant to the RimWorld modding community.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.
