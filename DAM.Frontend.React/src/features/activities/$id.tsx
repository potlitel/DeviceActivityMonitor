import { getRouteApi } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { ArrowLeft, Usb, Clock, HardDrive, FileText, Calendar } from 'lucide-react'
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

const route = getRouteApi('/_authenticated/activities/$id')

export function ActivityDetail() {
  const params = route.useParams()
  const navigate = route.useNavigate()
  
  const { data: activity, isLoading, error } = useQuery({
    queryKey: ['activity', params.id],
    queryFn: () => activitiesApi.getById(Number(params.id)),
    enabled: !!params.id,
  })

  return (
    <>
      <Header>
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate({ to: '/activities' })}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-2xl font-bold">Activity Details</h1>
            <p className="text-muted-foreground">View device activity information</p>
          </div>
        </div>
      </Header>

      <Main className="flex flex-1 flex-col gap-4 sm:gap-6">
        {isLoading && (
          <Card>
            <CardContent className="pt-6">
              <p className="text-center text-muted-foreground">Loading activity...</p>
            </CardContent>
          </Card>
        )}

        {error && (
          <Card>
            <CardContent className="pt-6">
              <p className="text-center text-destructive">Error loading activity</p>
            </CardContent>
          </Card>
        )}

        {activity && (
          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Usb className="h-5 w-5" />
                  Device Information
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Serial Number</p>
                    <p className="text-lg font-semibold">{activity.serialNumber}</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Model</p>
                    <p className="text-lg font-semibold">{activity.model}</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Status</p>
                    <Badge variant={activity.status === 'Active' ? 'default' : 'secondary'}>
                      {activity.status}
                    </Badge>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Total Capacity</p>
                    <p className="text-lg font-semibold">{(activity.totalCapacityMB / 1024).toFixed(0)} GB</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Clock className="h-5 w-5" />
                  Time Information
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Inserted At</p>
                    <p className="text-lg font-semibold">
                      {new Date(activity.insertedAt).toLocaleString()}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Extracted At</p>
                    <p className="text-lg font-semibold">
                      {activity.extractedAt 
                        ? new Date(activity.extractedAt).toLocaleString() 
                        : 'Still connected'}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Duration</p>
                    <p className="text-lg font-semibold">
                      {activity.timeInserted || 'N/A'}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <HardDrive className="h-5 w-5" />
                  Storage Data
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Initial Available</p>
                    <p className="text-lg font-semibold">{(activity.initialAvailableMB / 1024).toFixed(2)} GB</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Final Available</p>
                    <p className="text-lg font-semibold">
                      {activity.finalAvailableMB 
                        ? `${(activity.finalAvailableMB / 1024).toFixed(2)} GB` 
                        : 'N/A'}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">MB Copied</p>
                    <p className="text-lg font-semibold text-green-600">
                      +{(activity.megabytesCopied / 1024).toFixed(2)} GB
                    </p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">MB Deleted</p>
                    <p className="text-lg font-semibold text-red-600">
                      -{activity.megabytesDeleted} MB
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <FileText className="h-5 w-5" />
                  Files
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid gap-4 md:grid-cols-2">
                  <div>
                    <p className="text-sm font-medium text-muted-foreground mb-2">Files Copied</p>
                    <div className="max-h-32 overflow-auto space-y-1">
                      {activity.filesCopied.length > 0 ? (
                        activity.filesCopied.map((file, i) => (
                          <p key={i} className="text-sm truncate" title={file}>
                            {file}
                          </p>
                        ))
                      ) : (
                        <p className="text-sm text-muted-foreground">No files copied</p>
                      )}
                    </div>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground mb-2">Files Deleted</p>
                    <div className="max-h-32 overflow-auto space-y-1">
                      {activity.filesDeleted.length > 0 ? (
                        activity.filesDeleted.map((file, i) => (
                          <p key={i} className="text-sm truncate" title={file}>
                            {file}
                          </p>
                        ))
                      ) : (
                        <p className="text-sm text-muted-foreground">No files deleted</p>
                      )}
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        )}
      </Main>
    </>
  )
}
