import { getRouteApi } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { Activity, Usb, Clock, HardDrive, Calendar } from 'lucide-react'
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
import { activitiesApi, type DeviceActivity } from '@/lib/api'

const route = getRouteApi('/_authenticated/activities/')

export function Activities() {
  const search = route.useSearch()
  const navigate = route.useNavigate()
  
  const pageNumber = search.pageNumber || 1
  const pageSize = search.pageSize || 10
  const serialFilter = search.serial as string | undefined

  const { data, isLoading, error } = useQuery({
    queryKey: ['activities', pageNumber, pageSize, serialFilter],
    queryFn: () => activitiesApi.getAll({ 
      pageNumber, 
      pageSize,
      serialNumber: serialFilter 
    }),
  })

  return (
    <Main className="flex flex-1 flex-col gap-4 sm:gap-6">
      <div className="flex flex-wrap items-end justify-between gap-2">
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Device Activities</h2>
          <p className="text-muted-foreground">
            Monitor USB device connections and data transfers
          </p>
        </div>
      </div>

      {isLoading && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-center text-muted-foreground">Loading activities...</p>
          </CardContent>
        </Card>
      )}

      {error && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-center text-destructive">Error loading activities</p>
          </CardContent>
        </Card>
      )}

      {data && (
        <>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Total Activities</CardTitle>
                <Activity className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{data.totalCount}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Active</CardTitle>
                <Usb className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {data.items.filter(a => a.status === 'Active').length}
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Completed</CardTitle>
                <HardDrive className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {data.items.filter(a => a.status === 'Completed').length}
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Data Transferred</CardTitle>
                <Clock className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {(data.items.reduce((acc, a) => acc + a.megabytesCopied, 0) / 1024).toFixed(2)} GB
                </div>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Activity History</CardTitle>
              <CardDescription>
                Showing {data.items.length} of {data.totalCount} activities
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="relative w-full overflow-auto">
                <table className="w-full caption-bottom text-sm">
                  <thead className="[&_tr]:border-b">
                    <tr className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted">
                      <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">ID</th>
                      <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Serial Number</th>
                      <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Model</th>
                      <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Inserted</th>
                      <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Status</th>
                      <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">MB Copied</th>
                      <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="[&_tr:last-child]:border-0">
                    {data.items.map((activity) => (
                      <ActivityRow 
                        key={activity.id} 
                        activity={activity} 
                        onClick={() => navigate({ to: '/activities/$id', params: { id: activity.id.toString() } })}
                      />
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
        </>
      )}
    </Main>
  )
}

function ActivityRow({ activity, onClick }: { activity: DeviceActivity; onClick: () => void }) {
  return (
    <tr className="border-b transition-colors hover:bg-muted/50">
      <td className="p-4">{activity.id}</td>
      <td className="p-4 font-medium">{activity.serialNumber}</td>
      <td className="p-4">{activity.model}</td>
      <td className="p-4">
        <div className="flex items-center gap-2">
          <Calendar className="h-4 w-4 text-muted-foreground" />
          {new Date(activity.insertedAt).toLocaleDateString()}
        </div>
      </td>
      <td className="p-4">
        <Badge variant={activity.status === 'Active' ? 'default' : 'secondary'}>
          {activity.status}
        </Badge>
      </td>
      <td className="p-4">{(activity.megabytesCopied / 1024).toFixed(2)} GB</td>
      <td className="p-4">
        <Button variant="ghost" size="sm" onClick={onClick}>
          View
        </Button>
      </td>
    </tr>
  )
}
