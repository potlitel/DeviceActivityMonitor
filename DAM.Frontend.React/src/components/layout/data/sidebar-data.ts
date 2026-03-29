import {
  LayoutDashboard,
  Monitor,
  Receipt,
  ClipboardList,
  FileText,
  Settings,
  HelpCircle,
  UserCog,
  Bell,
  Palette,
  Usb,
} from 'lucide-react'
import { useAuthStore } from '@/stores/auth-store'
import { type SidebarData } from '../types'

const getUserFromStore = () => {
  const { user } = useAuthStore.getState()
  return {
    name: user?.username || 'DAM User',
    email: user?.email || 'user@dam.com',
    avatar: '/avatars/shadcn.jpg',
  }
}

export const sidebarData: SidebarData = {
  user: getUserFromStore(),
  teams: [
    {
      name: 'Device Activity Monitor',
      logo: Usb,
      plan: 'DAM System',
    },
  ],
  navGroups: [
    {
      title: 'General',
      items: [
        {
          title: 'Dashboard',
          url: '/',
          icon: LayoutDashboard,
        },
        {
          title: 'Activities',
          url: '/activities',
          icon: Usb,
        },
        {
          title: 'Presence',
          url: '/presence',
          icon: Monitor,
        },
      ],
    },
    {
      title: 'Billing',
      items: [
        {
          title: 'Invoices',
          url: '/invoices',
          icon: Receipt,
        },
      ],
    },
    {
      title: 'System',
      items: [
        {
          title: 'System Events',
          url: '/system-events',
          icon: ClipboardList,
        },
        {
          title: 'Audit Logs',
          url: '/audit',
          icon: FileText,
        },
      ],
    },
    {
      title: 'Settings',
      items: [
        {
          title: 'Settings',
          icon: Settings,
          items: [
            {
              title: 'Profile',
              url: '/settings',
              icon: UserCog,
            },
            {
              title: 'Appearance',
              url: '/settings/appearance',
              icon: Palette,
            },
            {
              title: 'Notifications',
              url: '/settings/notifications',
              icon: Bell,
            },
          ],
        },
        {
          title: 'Help Center',
          url: '/help-center',
          icon: HelpCircle,
        },
      ],
    },
  ],
}
