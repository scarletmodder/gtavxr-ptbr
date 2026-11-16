# Clicks the first button or tab with the given name in a process window (UI Automation), for documentation screenshots.
[CmdletBinding()]
param([Parameter(Mandatory = $true)][int]$ProcessId, [Parameter(Mandatory = $true)][string]$Name)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
$root = [System.Windows.Automation.AutomationElement]::RootElement
$byProcess = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $ProcessId)
$window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $byProcess)
if (-not $window) { throw 'Window not found' }
$byName = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty, $Name)
$element = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $byName)
if (-not $element) { throw "Element '$Name' not found" }
$invoke = $null
if ($element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invoke)) { $invoke.Invoke(); Write-Host "Invoked $Name"; return }
$select = $null
if ($element.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$select)) { $select.Select(); Write-Host "Selected $Name"; return }
# Text blocks inside custom buttons: click the parent that can be invoked.
$walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
$parent = $walker.GetParent($element)
while ($parent) {
    if ($parent.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invoke)) { $invoke.Invoke(); Write-Host "Invoked parent of $Name"; return }
    $parent = $walker.GetParent($parent)
}
throw "Element '$Name' cannot be clicked"
