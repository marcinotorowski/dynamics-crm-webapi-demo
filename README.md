# Dynamics CRM Web API Demo
This console app shows how to use the API of Dynamics CRM together with fetchXml query syntax and pagination. This repo shows how to achieve this without any external libraries (only Newtonsoft.Json required for parsing) and is minimalistic and cross-platform.

Points of interest:
* Authentication ([`Web/DynamicsCrmWebApiRequest.cs`](Web/DynamicsCrmWebApiRequest.cs))
* Building `fetchXml` queries ([`Domain/DynamicsCrmFetchXmlBuilder.cs`](Domain/DynamicsCrmFetchXmlBuilder.cs))
* Pagination ([`Web/DynamicsCrmWebApiResponse.cs`](Web/DynamicsCrmWebApiResponse.cs))
* Throwing meaningful exceptions in case of a bad request

In order to run the app, you have to have valid Azure App credentials (tenant, client/application ID and client secret) as well as Dynamics CRM URL. In order to authorize, there must be also a valid application user assigned to the registered app. The credentials should be changed in `Program.cs` before running.

## Simple output
The console shows audit logs from last 30 days, showing the date and the name of the user. The data is paginated (queried in chunks of 200 per page).

The results may look like this:

    00:00:00.0000264 Getting page #1 with no more than 200 entries...
    00:00:01.9639383 Received 200 entries.
    00:00:01.9779352 Getting page #2 with no more than 200 entries...
    00:00:02.5873344 Received 200 entries.
    00:00:02.5891197 Getting page #3 with no more than 200 entries...
    00:00:03.2735461 Received 200 entries.
    00:00:03.2739479 Getting page #4 with no more than 200 entries...
    00:00:03.9644486 Received 200 entries.
    00:00:03.9649688 Getting page #5 with no more than 200 entries...
    00:00:04.6026443 Received 200 entries.
    00:00:04.6030123 Getting page #6 with no more than 200 entries...
    00:00:05.1927297 Received 200 entries.
    00:00:05.1930468 Getting page #7 with no more than 200 entries...
    00:00:05.5917577 Received 112 entries.
    00:00:05.5922383 Finished! Received 1312 entries on 7 pages.
    First 5 items:
     * User='John Smith', Date='12/29/2020 00:23:40'
     * User='Andrew Trump', Date='12/29/2020 00:38:05'
     * User='Sandra O'Neil', Date='12/29/2020 01:08:46'
     * User='George Lam', Date='12/29/2020 01:28:42'
     * User='Rhonda Rowe, Date='12/29/2020 02:41:13'

## Further links and reading
* [Use FetchXML to query data (Microsoft Dataverse) - Power Apps | Microsoft Docs](https://docs.microsoft.com/en-us/powerapps/developer/data-platform/use-fetchxml-construct-query)
* [Page large result sets with FetchXML (Microsoft Dataverse) - Power Apps | Microsoft Docs](https://docs.microsoft.com/en-us/powerapps/developer/data-platform/org-service/page-large-result-sets-with-fetchxml)

## License
MIT