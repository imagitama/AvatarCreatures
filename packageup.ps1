$username = "Jared";
$pathToPlugin = "C:\Users\$username\AppData\Roaming\Thunderstore Mod Manager\DataFolder\LethalCompany\profiles\Default\BepInEx\plugins\PeanutBuddha-AvatarCreatures";

$pathToDll = "./bin/Debug/netstandard2.1/CreatureModels.dll"
$pathToModSrc = "./Mod"

Write-Host "Copying DLL from $pathToDll to $pathToModSrc"

cp $pathToDll $pathToModSrc

$pathToChangelog = "./CHANGELOG.md"

Write-Host "Copying CHANGELOG from $pathToChangelog to $pathToModSrc"

cp $pathToChangelog $pathToModSrc

$src = $pathToModSrc
$dest = $pathToPlugin

Write-Host "Copying files from $src to $dest"

cp -R "$src/*" $dest
