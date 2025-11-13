'use client'

import {Button} from '@heroui/button';
import {triggerError} from '@/lib/actions/error-actions';
import {useState, useTransition} from 'react';
import {handleError} from '@/lib/util';

export default function ErrorButtons() {
    const [pending, startTransition] = useTransition();
    const [target, setTarget] = useState(0)

    const onClick = (code: number) => {
        setTarget(code)
        startTransition(async () => {
            const {error} = await triggerError(code)
            if (error) handleError(error)
            setTarget(0)
        })
    }

    return (
        <div className='flex gap-6 items-center mt-6 w-full justify-center'>
            {[400, 401, 403, 404, 500].map(code => (
                <Button
                    className='rounded-full min-w-[12%]'
                    key={code}
                    color='primary'
                    type='button'
                    onPress={() => onClick(code)}
                    isLoading={pending && target === code}
                >
                    Test {code}
                </Button>
            ))}
        </div>
    );
}