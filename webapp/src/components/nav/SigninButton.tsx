'use client'

import {Button} from '@heroui/button';
import {signIn} from 'next-auth/react';

export default function SigninButton() {
    return (
        <Button
            color='secondary'
            variant='bordered'
            className='rounded-full w-[20%]'
            type='button'
            onPress={() => signIn('keycloak', {
                redirectTo: '/questions'}, {prompt: 'login'})}
        >
            Signin
        </Button>
    );
}