# Copilot Instructions for Hidden Enemy Bio Mod Development

## Mod Overview and Purpose

The Hidden Enemy Bio mod for RimWorld enhances gameplay by making the bio of enemies obscure until interaction or recruitment. This adds an element of mystery and strategy as players will not have foreknowledge of enemy stats, skills, or traits. It creates a more immersive and challenging experience for players who now must make decisions based on limited information in combat and recruitment scenarios.

## Key Features and Systems

- **Character Card Obfuscation**: The mod alters the character card display to hide crucial bio information until certain conditions are met.
  
- **Skill Anonymity**: Enemy skills are obscured, preventing players from viewing enemy proficiency before recruitment.
  
- **XML Configurability**: Mod settings are configurable through XML, allowing customization of hiding mechanisms and conditions.
  
- **Seamless Integration**: Modifies existing game components with minimal disruption through Harmony patches.

## Coding Patterns and Conventions

### General Conventions

- Use `PascalCase` for class and method names.
- Use `camelCase` for local variables and method parameters.
- Encapsulate fields in classes where necessary and prefer `private` access specifiers unless otherwise warranted.
- Ensure methods are well-documented with XML comments for maintainability.

### Specific Patterns

- Static helper utility classes such as `HiddenBioUtil` and `HiddenCharacterCardUtility` are used to isolate functionality related to the bio obfuscation logic.
- Declaration of mod settings is done through an internal settings class `Settings` which inherits from `ModSettings`.

## XML Integration

- XML files are utilized to define configurable aspects of the mod, such as the criteria for revealing hidden information.
- Ensure proper schema adherence and use of XML attributes to manage settings through RimWorld's mod configuration infrastructure.

## Harmony Patching

- **Harmony Basics**: Make use of Harmony library to apply runtime modifications to the game code without altering the original assemblies.
- **Patch Structure**: Typically create patches using `HarmonyPatch` attribute, targeting specific methods within the game's assemblies to inject custom functionality.
- **Patch Examples**: Use patches in `SkillUI_DrawSkillsOf` to control the display of skills and `CharacterCardUtility_DrawCharacterCard` to manipulate the character card layout.

## Suggestions for Copilot

When using GitHub Copilot to generate code snippets for this project:
1. **Utility Function Suggestion**: Request Copilot to assist in generating utility functions in `HiddenBioUtil` for common operations like revealing character bios conditionally.
2. **Harmony Patch Generation**: Encourage Copilot to help draft Harmony patch methods for particular game events or UI elements that need modification.
3. **XML Configuration Setup**: Utilize Copilot for iterating on XML structure, ensuring it meets the needs for modularity in setting configurations.
4. **Error Handling Routines**: Seek Copilot suggestions for robust error handling and logging to assist in debugging during runtime.

By following the conventions and utilizing Copilot effectively, developers can contribute to and enhance the Hidden Enemy Bio mod with safety and efficiency.
