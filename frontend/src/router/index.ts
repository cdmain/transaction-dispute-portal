import { createRouter, createWebHistory } from 'vue-router'
import TransactionsView from '@/views/TransactionsView.vue'
import DisputesView from '@/views/DisputesView.vue'
import DisputeDetailView from '@/views/DisputeDetailView.vue'
import DashboardView from '@/views/DashboardView.vue'
import LoginView from '@/views/LoginView.vue'
import RegisterView from '@/views/RegisterView.vue'
import { getAuthToken } from '@/utils/authStorage'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { requiresGuest: true }
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView,
      meta: { requiresGuest: true }
    },
    {
      path: '/',
      name: 'dashboard',
      component: DashboardView,
      meta: { requiresAuth: true }
    },
    {
      path: '/transactions',
      name: 'transactions',
      component: TransactionsView,
      meta: { requiresAuth: true }
    },
    {
      path: '/disputes',
      name: 'disputes',
      component: DisputesView,
      meta: { requiresAuth: true }
    },
    {
      path: '/disputes/:id',
      name: 'dispute-detail',
      component: DisputeDetailView,
      meta: { requiresAuth: true }
    }
  ]
})

// Navigation guards
router.beforeEach((to, _from, next) => {
  const isAuthenticated = !!getAuthToken()

  if (to.meta.requiresAuth && !isAuthenticated) {
    // Redirect to login if auth required but not logged in
    next({ name: 'login' })
  } else if (to.meta.requiresGuest && isAuthenticated) {
    // Redirect to dashboard if logged in user tries to access login/register
    next({ name: 'dashboard' })
  } else {
    next()
  }
})

export default router
