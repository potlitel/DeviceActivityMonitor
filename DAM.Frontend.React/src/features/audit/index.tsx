import { getRouteApi } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { FileText, User, Globe, Clock } from 'lucide-react'
import { Header } from '@/components/layout/header'
import { Main } from '@/components/layout/main'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { auditApi, type AuditLog } from '@/lib/api'

const route = getRouteApi('/_authenticated/audit/')

export function AuditLogs() {
  const search = route.useSearch()
  const navigate = route.useNavigate()
  
  const pageNumber = search.pageNumber || 1
  const pageSize = search.pageSize || 10

  const { data, isLoading, error } = useQuery({
    queryKey: ['audit', pageNumber, pageSize],
    queryFn: () => auditApi.getAll({ pageNumber, pageSize }),
  })

  const getMethodColor = (method: string) => {
    switch (method.toUpperCase()) {
      case 'GET': return 'bg-blue-100 text-blue-800'
      case 'POST': return 'bg-green-100 text-green-800'
      case 'PUT': return 'bg-yellow-100 text-yellow-800'
      case 'DELETE': return 'bg-red-100 text-red-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  return (
    <Main className="flex flex-1 flex-col gap-4 sm:gap-6">
      <div className="flex flex-wrap items-end justify-between gap-2">
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Audit Logs</h2>
          <p className="text-muted-foreground">
            System audit trail and access logs
          </p>
        </div>
      </div>

      {isLoading && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-center text-muted-foreground">Loading audit logs...</p>
          </CardContent>
        </Card>
      )}

      {error && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-center text-destructive">Error loading audit logs</p>
          </CardContent>
        </Card>
      )}

      {data && (
        <div className="grid gap-4 md:grid-cols-3">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Logs</CardTitle>
              <FileText className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{data.totalCount}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Unique Users</CardTitle>
              <User className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {new Set(data.items.map(l => l.userId)).size}
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Last 24h</CardTitle>
              <Clock className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {data.items.filter(l => 
                  new Date(l.timestampUtc) > new Date(Date.now() - 24 * 60 * 60 * 1000)
                ).length}
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {data && (
        <Card>
          <CardHeader>
            <CardTitle>Audit Trail</CardTitle>
            <CardDescription>
              Showing {data.items.length} of {data.totalCount} entries
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="relative w-full overflow-auto">
              <table className="w-full caption-bottom text-sm">
                <thead className="[&_tr]:border-b">
                  <tr className="border-b transition-colors hover:bg-muted/50">
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">User</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Action</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Resource</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Method</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">IP Address</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Timestamp</th>
                  </tr>
                </thead>
                <tbody className="[&_tr:last-child]:border-0">
                  {data.items.map((log) => (
                    <tr key={log.id} className="border-b transition-colors hover:bg-muted/50">
                      <td className="p-4">
                        <div className="flex items-center gap-2">
                          <User className="h-4 w-4 text-muted-foreground" />
                          {log.username || log.userId}
                        </div>
                      </td>
                      <td className="p-4 font-medium">{log.action}</td>
                      <td className="p-4">
                        <div className="flex items-center gap-2">
                          <Globe className="h-4 w-4 text-muted-foreground" />
                          {log.resource}
                        </div>
                      </td>
                      <td className="p-4">
                        <span className={`px-2 py-1 rounded text-xs font-medium ${getMethodColor(log.httpMethod)}`}>
                          {log.httpMethod}
                        </span>
                      </td>
                      <td className="p-4">{log.ipAddress || 'N/A'}</td>
                      <td className="p-4">
                        {new Date(log.timestampUtc).toLocaleString()}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="flex items-center justify-end space-x-2 py-4">
              <Button
                variant="outline"
                size="sm"
                disabled={pageNumber <= 1}
                onClick={() => navigate({ search: { pageNumber: pageNumber - 1, pageSize } })}
              >
                Previous
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={pageNumber >= data.totalPages}
                onClick={() => navigate({ search: { pageNumber: pageNumber + 1, pageSize } })}
              >
                Next
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </Main>
  )
}
