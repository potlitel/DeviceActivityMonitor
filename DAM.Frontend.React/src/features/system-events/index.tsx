import { getRouteApi } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { Activity, AlertTriangle, Info, XCircle, Bug } from 'lucide-react'
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
import { eventsApi, type ServiceEvent } from '@/lib/api'

const route = getRouteApi('/_authenticated/system-events/')

export function SystemEvents() {
  const search = route.useSearch()
  const navigate = route.useNavigate()
  
  const pageNumber = search.pageNumber || 1
  const pageSize = search.pageSize || 10

  const { data, isLoading, error } = useQuery({
    queryKey: ['system-events', pageNumber, pageSize],
    queryFn: () => eventsApi.getAll({ pageNumber, pageSize }),
  })

  const getLevelIcon = (level: string) => {
    switch (level) {
      case 'Information': return <Info className="h-4 w-4 text-blue-500" />
      case 'Warning': return <AlertTriangle className="h-4 w-4 text-yellow-500" />
      case 'Error': return <XCircle className="h-4 w-4 text-red-500" />
      default: return <Bug className="h-4 w-4" />
    }
  }

  const getLevelVariant = (level: string): 'default' | 'secondary' | 'destructive' => {
    switch (level) {
      case 'Information': return 'default'
      case 'Warning': return 'secondary'
      case 'Error': return 'destructive'
      default: return 'secondary'
    }
  }

  return (
    <Main className="flex flex-1 flex-col gap-4 sm:gap-6">
      <div className="flex flex-wrap items-end justify-between gap-2">
        <div>
          <h2 className="text-2xl font-bold tracking-tight">System Events</h2>
          <p className="text-muted-foreground">
            System logs and diagnostic events
          </p>
        </div>
      </div>

      {isLoading && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-center text-muted-foreground">Loading events...</p>
          </CardContent>
        </Card>
      )}

      {error && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-center text-destructive">Error loading events</p>
          </CardContent>
        </Card>
      )}

      {data && (
        <div className="grid gap-4 md:grid-cols-3">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Events</CardTitle>
              <Activity className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{data.totalCount}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Errors</CardTitle>
              <XCircle className="h-4 w-4 text-red-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-red-500">
                {data.items.filter(e => e.level === 'Error').length}
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Warnings</CardTitle>
              <AlertTriangle className="h-4 w-4 text-yellow-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-yellow-500">
                {data.items.filter(e => e.level === 'Warning').length}
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {data && (
        <Card>
          <CardHeader>
            <CardTitle>Event Log</CardTitle>
            <CardDescription>
              Showing {data.items.length} of {data.totalCount} events
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="relative w-full overflow-auto">
              <table className="w-full caption-bottom text-sm">
                <thead className="[&_tr]:border-b">
                  <tr className="border-b transition-colors hover:bg-muted/50">
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">ID</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Level</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Source</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Message</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Timestamp</th>
                  </tr>
                </thead>
                <tbody className="[&_tr:last-child]:border-0">
                  {data.items.map((event) => (
                    <tr key={event.id} className="border-b transition-colors hover:bg-muted/50">
                      <td className="p-4">{event.id}</td>
                      <td className="p-4">
                        <Badge variant={getLevelVariant(event.level)} className="flex items-center gap-1 w-fit">
                          {getLevelIcon(event.level)}
                          {event.level}
                        </Badge>
                      </td>
                      <td className="p-4">{event.source}</td>
                      <td className="p-4 max-w-md truncate" title={event.message}>
                        {event.message}
                      </td>
                      <td className="p-4">
                        {new Date(event.timestamp).toLocaleString()}
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
