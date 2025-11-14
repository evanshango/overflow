import {Button} from '@heroui/button';

export default function SignupButton() {
    const clientId = 'nextjs'
    const issuer = process.env.AUTH_KEYCLOAK_ISSUER
    const redirectURL = process.env.AUTH_URL

    const signupURL = `${issuer}/protocol/openid-connect/registrations` +
        `?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectURL!)}` +
        `&response_type=code&scope=openid`

    return (
        <Button as='a' href={signupURL} color='secondary' className='rounded-full w-[20%]'>
            Signup
        </Button>
    );
}