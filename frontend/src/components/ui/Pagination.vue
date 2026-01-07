<template>
  <nav class="flex items-center justify-between px-4 py-3 bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700 rounded-b-lg sm:px-6">
    <div class="flex items-center">
      <p class="text-sm text-gray-700 dark:text-gray-300">
        Showing
        <span class="font-medium text-gray-900 dark:text-white">{{ startItem }}</span>
        to
        <span class="font-medium text-gray-900 dark:text-white">{{ endItem }}</span>
        of
        <span class="font-medium text-gray-900 dark:text-white">{{ totalCount }}</span>
        results
      </p>
    </div>
    <div class="flex items-center space-x-2">
      <button
        @click="$emit('previous')"
        :disabled="!hasPreviousPage"
        class="relative inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md hover:bg-gray-50 dark:hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        Previous
      </button>
      <span class="text-sm text-gray-700 dark:text-gray-300">
        Page {{ page }} of {{ totalPages }}
      </span>
      <button
        @click="$emit('next')"
        :disabled="!hasNextPage"
        class="relative inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md hover:bg-gray-50 dark:hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        Next
      </button>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}>()

defineEmits<{
  (e: 'next'): void
  (e: 'previous'): void
}>()

const startItem = computed(() => {
  if (props.totalCount === 0) return 0
  return (props.page - 1) * props.pageSize + 1
})

const endItem = computed(() => {
  const end = props.page * props.pageSize
  return end > props.totalCount ? props.totalCount : end
})
</script>
