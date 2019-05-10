Get-WMIObject -class Win32_UserProfile | Where {(!$_.Special) -and ($_.ConvertToDateTime($_.LastUseTime) -lt (Get-Date).AddDays(-5))} | Remove-WmiObject


ConnectionOptions connectionOptions = new ConnectionOptions()
ManagementScope scope = new ManagementScope("\\\\" + localhost + "\\" + CIM_V2, connectionOptions);
scope.Connect();
ObjectQuery objectQuery = new ObjectQuery("select " + "*" + " from " + "Win32_UserProfile");
ObjectQuery query2 = objectQuery;
using (var searcher = new ManagementObjectSearcher(scope, query, options))
{
	searcher.Get();
}.using (System.Management.Automation.PowerShell powershell = System.Management.Automation.PowerShell.Create())
{
	powershell.AddCommand("Where");
	powershell.AddArgument((_.Special) && (_.ConvertToDateTime(_.LastUseTime) < (using (System.Management.Automation.PowerShell powershell = System.Management.Automation.PowerShell.Create())
	{
		powershell.AddCommand("Get-Date");
		powershell.Invoke();
	}).AddDays(-5));
	);
	powershell.AddArgument();
	powershell.Invoke();
}.using (System.Management.Automation.PowerShell powershell = System.Management.Automation.PowerShell.Create())
{
	powershell.AddCommand("Remove-WmiObject");
	powershell.AddArgument();
	powershell.Invoke();
}