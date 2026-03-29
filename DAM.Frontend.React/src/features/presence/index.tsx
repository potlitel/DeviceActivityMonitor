import { getRouteApi } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { MapPin, Clock, Usb, Calendar } from 'lucide-react'
import { Header } from '@/components/layout/header'
import { Main } from '@/components/layout/main'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { presenceApi, type DevicePresence } from '@/lib/api'

const route = getRouteApi('/_authenticated/presence/')

export function Presence() {
  const search = route.useSearch()
  const navigate = route.useNavigate()
  
  const pageNumber = search.pageNumber || 1
  const pageSize = search.pageSize || 10

  const { data, isLoading, error } = useQuery({
    queryKey: ['presence', pageNumber, pageSize],
    queryFn: () => presenceApi.getAll({ pageNumber, pageSize }),
  })

  return (
    <Main className="flex flex-1 flex-col gap-4 sm:gap-6">
      <div className="flex flex-wrap items-end justify-between gap-2">
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Device Presence</h2>
          <p className="text-muted-foreground">
            Real-time device presence detection events
          </p>
        </div>
      </div>

      {isLoading && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-center text-muted-foreground">Loading presence data...</p>
          </CardContent>
        </Card>
      )}

      {error && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-center text-destructive">Error loading presence data</p>
          </CardContent>
        </Card>
      )}

      {data && (
        <div className="grid gap-4 md:grid-cols-3">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Events</CardTitle>
              <MapPin className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{data.totalCount}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Unique Devices</CardTitle>
              <Usb className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {new Set(data.items.map(p => p.serialNumber)).size}
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
                {data.items.filter(p => 
                  new Date(p.timestamp) > new Date(Date.now() - 24 * 60 * 60 * 1000)
                ).length}
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {data && (
        <Card>
          <CardHeader>
            <CardTitle>Presence Events</CardTitle>
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
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Serial Number</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Timestamp</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Activity ID</th>
                  </tr>
                </thead>
                <tbody className="[&_tr:last-child]:border-0">
                  {data.items.map((presence) => (
                    <tr key={presence.id} className="border-b transition-colors hover:bg-muted/50">
                      <td className="p-4">{presence.id}</td>
                      <td className="p-4 font-medium">{presence.serialNumber}</td>
                      <td className="p-4">
                        <div className="flex items-center gap-2">
                          <Calendar className="h-4 w-4 text-muted-foreground" />
                          {new Date(presence.timestamp).toLocaleString()}
                        </div>
                      </td>
                      <td className="p-4">{presence.deviceActivityId}</td>
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
