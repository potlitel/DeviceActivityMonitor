import { getRouteApi } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { Receipt, DollarSign, Calendar, CreditCard } from 'lucide-react'
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
import { invoicesApi, type Invoice } from '@/lib/api'

const route = getRouteApi('/_authenticated/invoices/')

export function Invoices() {
  const search = route.useSearch()
  const navigate = route.useNavigate()
  
  const pageNumber = search.pageNumber || 1
  const pageSize = search.pageSize || 10

  const { data, isLoading, error } = useQuery({
    queryKey: ['invoices', pageNumber, pageSize],
    queryFn: () => invoicesApi.getAll({ pageNumber, pageSize }),
  })

  return (
    <Main className="flex flex-1 flex-col gap-4 sm:gap-6">
      <div className="flex flex-wrap items-end justify-between gap-2">
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Invoices</h2>
          <p className="text-muted-foreground">
            View and manage device activity invoices
          </p>
        </div>
      </div>

      {isLoading && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-center text-muted-foreground">Loading invoices...</p>
          </CardContent>
        </Card>
      )}

      {error && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-center text-destructive">Error loading invoices</p>
          </CardContent>
        </Card>
      )}

      {data && (
        <div className="grid gap-4 md:grid-cols-3">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Invoices</CardTitle>
              <Receipt className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{data.totalCount}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Amount</CardTitle>
              <DollarSign className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                ${data.items.reduce((acc, inv) => acc + inv.totalAmount, 0).toFixed(2)}
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Average Invoice</CardTitle>
              <CreditCard className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                ${data.totalCount > 0 
                  ? (data.items.reduce((acc, inv) => acc + inv.totalAmount, 0) / data.totalCount).toFixed(2)
                  : '0.00'}
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {data && (
        <Card>
          <CardHeader>
            <CardTitle>Invoice List</CardTitle>
            <CardDescription>
              Showing {data.items.length} of {data.totalCount} invoices
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="relative w-full overflow-auto">
              <table className="w-full caption-bottom text-sm">
                <thead className="[&_tr]:border-b">
                  <tr className="border-b transition-colors hover:bg-muted/50">
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">ID</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Serial Number</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Date</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Amount</th>
                    <th className="h-12 px-4 text-left align-middle font-medium text-muted-foreground">Description</th>
                  </tr>
                </thead>
                <tbody className="[&_tr:last-child]:border-0">
                  {data.items.map((invoice) => (
                    <tr key={invoice.id} className="border-b transition-colors hover:bg-muted/50">
                      <td className="p-4">{invoice.id}</td>
                      <td className="p-4 font-medium">{invoice.serialNumber}</td>
                      <td className="p-4">
                        <div className="flex items-center gap-2">
                          <Calendar className="h-4 w-4 text-muted-foreground" />
                          {new Date(invoice.timestamp).toLocaleDateString()}
                        </div>
                      </td>
                      <td className="p-4 font-semibold text-green-600">
                        ${invoice.totalAmount.toFixed(2)}
                      </td>
                      <td className="p-4 max-w-xs truncate" title={invoice.description}>
                        {invoice.description}
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
