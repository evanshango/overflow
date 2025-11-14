'use server'

import {fetchClient} from '@/lib/fetchClient';

export async function triggerError(code: number) {
    return await fetchClient(`/api/v1/tests/errors?code=${code}`, 'GET')
}