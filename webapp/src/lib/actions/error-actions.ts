'use server'

import {fetchClient} from '@/lib/fetchClient';

export async function triggerError(code: number) {
    return await fetchClient(`/api/v1/questions/errors?code=${code}`, 'GET')
}