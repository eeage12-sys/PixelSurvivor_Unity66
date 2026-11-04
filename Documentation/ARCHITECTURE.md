# Current architecture

See CAMPAIGN_DESIGN.md for the implemented infinite-survival architecture and README.txt for installation.
LATEST_REQUIREMENTS.txt supersedes previous requirements. No 5-minute or 20-minute clear loop is active.

Core/GameManager -> CampaignController -> StageDirector / CombatArsenal / FieldSystem / SupportUnitManager.
CampaignController -> EquipmentManager -> TreasureChestSystem.
CampaignController -> SaveManager -> MetaProgress / per-character, per-stage SurvivalRecord.
CampaignUI builds Main Menu -> Character Select -> Stage Select -> Run, plus shop/HUD/results.
Data/CharacterData, WeaponData, PassiveEquipmentData, StageData and CampaignData hold editable content.
PlayerMovement consumes one InputActionReference, PlayerSpriteAnimator uses selected character frames.
Editor/SurvivorSetup builds a new scene; Editor/CampaignSetup migrates an existing scene.
