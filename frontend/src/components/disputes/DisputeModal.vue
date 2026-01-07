<template>
  <TransitionRoot appear :show="isOpen" as="template">
    <Dialog as="div" @close="closeModal" class="relative z-50">
      <TransitionChild
        as="template"
        enter="duration-300 ease-out"
        enter-from="opacity-0"
        enter-to="opacity-100"
        leave="duration-200 ease-in"
        leave-from="opacity-100"
        leave-to="opacity-0"
      >
        <div class="fixed inset-0 bg-black/25 backdrop-blur-sm" />
      </TransitionChild>

      <div class="fixed inset-0 overflow-y-auto">
        <div class="flex min-h-full items-center justify-center p-4 text-center">
          <TransitionChild
            as="template"
            enter="duration-300 ease-out"
            enter-from="opacity-0 scale-95"
            enter-to="opacity-100 scale-100"
            leave="duration-200 ease-in"
            leave-from="opacity-100 scale-100"
            leave-to="opacity-0 scale-95"
          >
            <DialogPanel 
              class="w-full max-w-lg transform overflow-hidden rounded-2xl bg-white p-6 text-left align-middle shadow-xl transition-all"
            >
              <DialogTitle as="h3" class="text-lg font-semibold leading-6 text-gray-900">
                File a Dispute
              </DialogTitle>

              <div v-if="transaction" class="mt-4 p-4 bg-gray-50 rounded-lg">
                <div class="flex justify-between items-start">
                  <div>
                    <p class="text-sm font-medium text-gray-900">{{ transaction.merchantName }}</p>
                    <p class="text-sm text-gray-500">{{ transaction.description }}</p>
                    <p class="text-xs text-gray-400 mt-1">Ref: {{ transaction.reference }}</p>
                  </div>
                  <div class="text-right">
                    <p class="text-lg font-semibold text-gray-900">
                      {{ formatAmount(transaction.amount) }}
                    </p>
                    <p class="text-xs text-gray-500">{{ transaction.currency }}</p>
                  </div>
                </div>
              </div>

              <form @submit.prevent="handleSubmit" class="mt-6 space-y-4">
                <div>
                  <label for="category" class="block text-sm font-medium text-gray-700">
                    Dispute Category *
                  </label>
                  <select
                    id="category"
                    v-model="form.category"
                    class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
                    required
                  >
                    <option :value="0">Unauthorized Transaction</option>
                    <option :value="1">Duplicate Charge</option>
                    <option :value="2">Incorrect Amount</option>
                    <option :value="3">Service Not Received</option>
                    <option :value="4">Product Not Received</option>
                    <option :value="5">Quality Issue</option>
                    <option :value="6">Refund Not Received</option>
                    <option :value="7">Fraud Suspected</option>
                    <option :value="8">Other</option>
                  </select>
                </div>

                <div>
                  <label for="reason" class="block text-sm font-medium text-gray-700">
                    Brief Reason *
                  </label>
                  <input
                    id="reason"
                    v-model="form.reason"
                    type="text"
                    maxlength="200"
                    class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
                    placeholder="e.g., I did not authorize this transaction"
                    required
                  />
                  <p v-if="errors.reason" class="mt-1 text-sm text-red-600">{{ errors.reason }}</p>
                </div>

                <div>
                  <label for="description" class="block text-sm font-medium text-gray-700">
                    Detailed Description *
                  </label>
                  <textarea
                    id="description"
                    v-model="form.description"
                    rows="4"
                    maxlength="2000"
                    class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
                    placeholder="Please provide as much detail as possible about why you are disputing this transaction..."
                    required
                  />
                  <p v-if="errors.description" class="mt-1 text-sm text-red-600">{{ errors.description }}</p>
                </div>

                <div>
                  <label for="amount" class="block text-sm font-medium text-gray-700">
                    Disputed Amount *
                  </label>
                  <div class="mt-1 relative rounded-md shadow-sm">
                    <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                      <span class="text-gray-500 sm:text-sm">R</span>
                    </div>
                    <input
                      id="amount"
                      v-model.number="form.disputedAmount"
                      type="number"
                      step="0.01"
                      min="0.01"
                      class="block w-full pl-7 pr-12 rounded-md border-gray-300 focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
                      required
                    />
                    <div class="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                      <span class="text-gray-500 sm:text-sm">ZAR</span>
                    </div>
                  </div>
                  <p v-if="errors.disputedAmount" class="mt-1 text-sm text-red-600">{{ errors.disputedAmount }}</p>
                </div>

                <div class="mt-6 flex justify-end space-x-3">
                  <button
                    type="button"
                    @click="closeModal"
                    class="inline-flex justify-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    :disabled="isSubmitting"
                    class="inline-flex justify-center rounded-md border border-transparent bg-primary-600 px-4 py-2 text-sm font-medium text-white hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <svg v-if="isSubmitting" class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    {{ isSubmitting ? 'Submitting...' : 'Submit Dispute' }}
                  </button>
                </div>
              </form>
            </DialogPanel>
          </TransitionChild>
        </div>
      </div>
    </Dialog>
  </TransitionRoot>
</template>

<script setup lang="ts">
import { reactive, watch } from 'vue'
import {
  TransitionRoot,
  TransitionChild,
  Dialog,
  DialogPanel,
  DialogTitle,
} from '@headlessui/vue'
import type { Transaction, CreateDisputeRequest } from '@/types'
import { CreateDisputeSchema } from '@/schemas'
import { useCreateDispute } from '@/composables'

const props = defineProps<{
  isOpen: boolean
  transaction: Transaction | null
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'success'): void
}>()

const createDisputeMutation = useCreateDispute()

const errors = reactive<Record<string, string>>({})

const form = reactive({
  category: 0,
  reason: '',
  description: '',
  disputedAmount: 0
})

watch(() => props.transaction, (newTransaction) => {
  if (newTransaction) {
    form.disputedAmount = newTransaction.amount
  }
}, { immediate: true })

function formatAmount(amount: number): string {
  return new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: 'ZAR'
  }).format(amount)
}

function closeModal() {
  Object.keys(errors).forEach(key => delete errors[key])
  form.category = 0
  form.reason = ''
  form.description = ''
  form.disputedAmount = 0
  emit('close')
}

async function handleSubmit() {
  if (!props.transaction) return

  // Clear previous errors
  Object.keys(errors).forEach(key => delete errors[key])

  const data: CreateDisputeRequest = {
    transactionId: props.transaction.id,
    customerId: props.transaction.customerId,
    reason: form.reason,
    description: form.description,
    category: form.category,
    disputedAmount: form.disputedAmount,
    currency: props.transaction.currency,
    transactionReference: props.transaction.reference || undefined,
    merchantName: props.transaction.merchantName
  }

  // Validate with Zod
  const result = CreateDisputeSchema.safeParse(data)
  if (!result.success) {
    result.error.errors.forEach(err => {
      const field = err.path[0] as string
      errors[field] = err.message
    })
    return
  }

  try {
    await createDisputeMutation.mutateAsync(data)
    emit('success')
    closeModal()
  } catch (error) {
    console.error('Failed to create dispute:', error)
  }
}

const isSubmitting = createDisputeMutation.isPending
</script>
